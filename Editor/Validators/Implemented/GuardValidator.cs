using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Extern;
using Validatox.Editor.Validators.Fix;
using Validatox.Meta;
using Validatox.Serializable;

namespace Validatox.Editor.Validators
{
    [CreateAssetMenu(menuName = "Validatox/Guard validator", fileName = "guard_validator")]
    public sealed class GuardValidator : Validator
    {
        [SerializeField] private bool validateAssets = true;

        [SerializeField] private bool allScenes = true;

        [SerializeField] private SceneReference[] scenes;

        public override void Validate(List<ValidationFailure> failures, Action<ValidationProgress> progress = null)
        {
            var assets = new List<UnityEngine.Object>();
            var progressVal = new ValidationProgress(nameof(GuardValidator), "Retrieving assets...");
            progress?.Invoke(progressVal);
            if (validateAssets)
            {
                var assetsObjs = ValidatoxTools.GetUnityObjectsInAssets();
                assets.AddRange(ValidatoxTools.GetAllBehavioursObjects(assetsObjs));
            }

            progress?.Invoke(progressVal.Doing("Retrieving scenes..."));
            var scenesPaths = new List<string>();
            if (allScenes)
            {
                if (EditorBuildSettings.scenes.Length <= 0)
                {
                }
                else
                {
                    scenesPaths.AddRange(from s in EditorBuildSettings.scenes select s.path);
                }
            }
            else if (scenes is not null && scenes.Length > 0)
            {
                scenesPaths.AddRange(from s in scenes select s.Path);
            }
            progressVal.Doing("Validating assets...");
            var totalLength = assets.Count + scenesPaths.Count;
            var count = 0;
            for (var i = 0; i < assets.Count; i++, count++)
            {
                var obj = assets[i];
                failures.AddRange(Guard(obj, obj, AssetDatabase.GetAssetPath(obj)));
                progress?.Invoke(progressVal.WithProgress((float)count / totalLength));
            }

            scenesPaths.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            if (scenesPaths.Count > 0)
            {
                for (var i = 0; i < scenesPaths.Count; i++, count++)
                {
                    var scene = scenesPaths[i];
                    progressVal.Doing("Validating scene: " + scene);
                    ValidatoxTools.ExecuteForComponentsInScene<MonoBehaviour>(scene, m =>
                    {
                        if (!m) return;
                        failures.AddRange(Guard(m, m.gameObject, scene));
                    });
                    progress?.Invoke(progressVal.WithProgress((float)count / totalLength));
                }
            }
        }

        protected override List<ValidationFailure> ValidateSingle(UnityEngine.Object obj, string scenePath)
        {
            var behaviours = ValidatoxTools.GetAllBehavioursObjects(new List<UnityEngine.Object>() { obj });
            var failures = new List<ValidationFailure>();
            foreach (var behaviour in behaviours)
            {
                var path = AssetDatabase.Contains(obj) ? AssetDatabase.GetAssetPath(obj) : scenePath;
                failures.AddRange(Guard(behaviour, obj, path));
            }
            return failures;
        }

        protected override void ForceSerialize(Validator dirty)
        {
            dirty = ExecuteMutex.isExecuting ? AssetDatabase.LoadAssetAtPath<Validator>(ExecuteMutex.executingAssetPath) : dirty;
            base.ForceSerialize(dirty);
        }

        protected override bool CanValidate()
        {
            if (ExecuteMutex.isExecuting)
            {
                NotifyLog(Log.Create("Cannot run multiple guarded validators at once!").Type(LogType.Warning));
                return false;
            }
            return true;
        }

        protected override void ValidationStart()
        {
            ExecuteMutex.isExecuting = true;
            ExecuteMutex.executingAssetPath = AssetDatabase.GetAssetPath(this);
        }

        protected override void ValidationEnd()
        {
            ExecuteMutex.isExecuting = false;
            ExecuteMutex.executingAssetPath = string.Empty;
        }

        private void NotifyLog(params Log[] logs)
        {
            var l = new List<Log>
            {
                Log.Create($"<b>{name}</b>")
            };
            l.AddRange(logs);
            ValidatoxLogEditorWindow.Notify(l.ToArray());
        }

        private ValidationFailure BuildFailure(GuardAttribute guarded, FieldInfo field, UnityEngine.Object behavior, Type classType, string path, string scene)
        {
            var logBuilder = new StringBuilder();
            if (scene == null)
            {
                logBuilder.Append("<color=#47c3e6><b>Prefab</b></color> > ");
            }
            else
            {
                logBuilder.Append("<color=#ac55e6><b>Scene Object</b></color> > ");
            }
            switch (guarded.SeverityType)
            {
                case GuardAttribute.Severity.Info:
                    logBuilder.AppendFormat("<b>{0}</b> field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;

                case GuardAttribute.Severity.Warning:
                    logBuilder.AppendFormat("<color=#E1BE32><b>{0}</b></color> field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;

                case GuardAttribute.Severity.Error:
                    logBuilder.AppendFormat("<color=#C8231E><b>{0}</b></color> field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;
            }
            return ValidationFailure.Of(this).CausedBy(behavior, scene).WithFix<MissingReferenceFix>(field.Name, new SerializableType(field.FieldType)).Reason(logBuilder.ToString());
        }

        private List<ValidationFailure> Guard(UnityEngine.Object behaviour, UnityEngine.Object parentObj, string path)
        {
            var failures = new List<ValidationFailure>();
            if (behaviour == null) return failures;
            var fields = behaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(behaviour);
                if (Attribute.GetCustomAttribute(field, typeof(GuardAttribute)) is GuardAttribute guarded)
                {
                    if (value is not null && value.GetType().IsValueType) continue;
                    if (value.IsNullOrDefault())
                    {
                        failures.Add(BuildFailure(guarded, field, behaviour, behaviour.GetType(), $"{path}/{parentObj.name}", AssetDatabase.Contains(parentObj) ? null : path));
                    }
                }
            }
            return failures;
        }

        private static class ExecuteMutex
        {
            public static string executingAssetPath;
            public static bool isExecuting;
        }
    }
}