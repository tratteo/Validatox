using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
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
            if (validateAssets) assets.AddRange(ValidatoxTools.GetUnityObjectsInAssets().ToArray());

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
                failures.AddRange(GuardRecursively(obj, obj, AssetDatabase.GetAssetPath(obj)));
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
                    ValidatoxTools.ExecuteForComponentsInScene<MonoBehaviour>(scene, m => failures.AddRange(GuardRecursively(m, m.gameObject, scene)));
                    progress?.Invoke(progressVal.WithProgress((float)count / totalLength));
                }
                Log($"Validated {(allScenes ? "all" : scenesPaths.Count)} scenes");
            }
        }

        protected override void SerializeIfDirty(Validator dirty)
        {
            dirty = ExecuteMutex.isExecuting ? AssetDatabase.LoadAssetAtPath<Validator>(ExecuteMutex.executingAssetPath) : dirty;
            base.SerializeIfDirty(dirty);
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

        private void Log(string message, LogType type = LogType.Log)
        {
            var validatoxLog = EditorWindow.GetWindow<ValidatoxLogEditorWindow>();
            if (validatoxLog is null) return;
            validatoxLog.NotifyLog($"<b>[GuardedValidator]</b> -> {message}", type);
        }

        private ValidationFailure BuildFailure(GuardAttribute guarded, FieldInfo field, Type parentClass, string path, bool isAsset)
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
                    logBuilder.AppendFormat("<b>{0}</b> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, parentClass, path);
                    break;

                case GuardAttribute.Severity.Warning:
                    logBuilder.AppendFormat("<color=yellow><b>{0}</b></color> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, parentClass, path);
                    break;

                case GuardAttribute.Severity.Error:
                    logBuilder.AppendFormat("<color=red><b>{0}</b></color> | Field <b>{1}</b> of class <b>{2}</b> on Object <b>{3}</b> is set to default value", guarded.Message, field.Name, parentClass, path);
                    break;
            }
            return ValidationFailure.Of(this).Reason(logBuilder.ToString()).By(path, field.Name);
        }

        private List<ValidationFailure> GuardRecursively(object behaviour, UnityEngine.Object parentObj, string path)
        {
            var failures = new List<ValidationFailure>();
            if (behaviour is null) return failures;
            var fields = behaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (Attribute.GetCustomAttribute(field, typeof(GuardAttribute)) is GuardAttribute guarded)
                {
                    if (field.GetValue(behaviour).IsNullOrDefault())
                    {
                        failures.Add(BuildFailure(guarded, field, behaviour.GetType(), $"{path}/{parentObj.name}", EditorUtility.IsPersistent(parentObj)));
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