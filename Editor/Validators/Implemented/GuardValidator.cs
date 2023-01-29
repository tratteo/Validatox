using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
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
                var singleList = new List<UnityEngine.Object>();
                var assetsObjs = ValidatoxTools.GetUnityObjectsInAssets();
                foreach (var a in assetsObjs)
                {
                    singleList.Add(a);
                    assets.AddRange(ValidatoxTools.GetAllBehavioursObjects(singleList));
                    singleList.Clear();
                }
            }

            progress?.Invoke(progressVal.Doing("Retrieving scenes..."));
            var scenesPaths = new List<string>();
            if (allScenes)
            {
                if (EditorBuildSettings.scenes.Length <= 0)
                {
                    Log("No scenes in build settings", LogType.Warning);
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
            Log($"Validated {assets.Count} assets");

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
                Log($"Validated {(allScenes ? "all" : scenesPaths.Count)} scenes");
            }
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
                Log("Cannot run multiple guarded validators at once!", LogType.Warning);
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

        private void Log(string message, LogType type = LogType.Log) => ValidatoxLogEditorWindow.NotifyLog($"<b>[GuardedValidator]</b> -> {message}", type);

        private ValidationFailure BuildFailure(GuardAttribute guarded, FieldInfo field, UnityEngine.Object parent, Type classType, string path, bool isAsset)
        {
            var logBuilder = new StringBuilder();
            if (isAsset)
            {
                logBuilder.Append("<color=cyan><b>Prefab</b></color> => ");
            }
            else
            {
                logBuilder.Append("<color=magenta><b>Scene Object</b></color> => ");
            }
            switch (guarded.SeverityType)
            {
                case GuardAttribute.Severity.Info:
                    logBuilder.AppendFormat("<b>{0}</b> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;

                case GuardAttribute.Severity.Warning:
                    logBuilder.AppendFormat("<color=yellow><b>{0}</b></color> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;

                case GuardAttribute.Severity.Error:
                    logBuilder.AppendFormat("<color=red><b>{0}</b></color> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, classType, path);
                    break;
            }
            return ValidationFailure.Of(this, parent).WithFix<MissingReferenceFix>(field.Name).Reason(logBuilder.ToString());
        }

        private List<ValidationFailure> Guard(UnityEngine.Object behaviour, UnityEngine.Object parentObj, string path)
        {
            var failures = new List<ValidationFailure>();
            if (behaviour is null) return failures;
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
                        failures.Add(BuildFailure(guarded, field, behaviour, behaviour.GetType(), $"{path}/{parentObj.name}", EditorUtility.IsPersistent(parentObj)));
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