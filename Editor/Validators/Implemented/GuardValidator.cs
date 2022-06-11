﻿using System;
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
    public class GuardValidator : Validator
    {
        [SerializeField] private bool validateAssets = true;
        [SerializeField] private bool allScenes = false;
        [SerializeField] private SceneReference[] scenes;

        public override void Validate(List<ValidatorFailure> failures, Action<IValidable.ValidationProgress> progress = null)
        {
            var assets = new List<UnityEngine.Object>();
            var managedProgress = progress is not null;
            if (!managedProgress)
            {
                progress = EditorProgressReport;
            }
            var progressVal = new IValidable.ValidationProgress(nameof(GuardValidator), "Retrieving assets...", 0);
            progress?.Invoke(progressVal);
            if (validateAssets) assets.AddRange(ValidatoxTools.GetUnityObjectsInAssets().ToArray());
            progressVal.description = "Retrieving scenes...";
            progress?.Invoke(progressVal);
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
            progressVal.description = "Validating assets...";
            var totalLength = assets.Count + scenesPaths.Count;
            var count = 0;
            for (var i = 0; i < assets.Count; i++, count++)
            {
                var obj = assets[i];
                failures.AddRange(GuardRecursively(obj, obj, AssetDatabase.GetAssetPath(obj)));
                progressVal.value = (float)count / totalLength;
                progress?.Invoke(progressVal);
            }
            Log($"Validated {assets.Count} assets");

            scenesPaths.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            if (scenesPaths.Count > 0)
            {
                for (var i = 0; i < scenesPaths.Count; i++, count++)
                {
                    var scene = scenesPaths[i];
                    progressVal.description = "Validating scene: " + scene;
                    ValidatoxTools.ExecuteForComponentsInScene<MonoBehaviour>(scene, m => failures.AddRange(GuardRecursively(m, m.gameObject, scene)));
                    progressVal.value = (float)count / totalLength;
                    progress?.Invoke(progressVal);
                }
                Log($"Validated {(allScenes ? "all" : scenesPaths.Count)} scenes");
            }
            if (!managedProgress)
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void EditorProgressReport(IValidable.ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.phase, progress.description, progress.value);

        private void Log(string message, LogType type = LogType.Log)
        {
            switch (type)
            {
                case LogType.Log:
                    Debug.Log($"<b>[GuardedValidator]</b> -> {message}");
                    break;

                case LogType.Warning:
                    Debug.LogWarning($"<b>[GuardedValidator]</b> -> {message}");
                    break;

                default:
                    Debug.LogError($"<b>[GuardedValidator]</b> -> {message}");
                    break;
            }
        }

        private ValidatorFailure BuildFailure(GuardAttribute guarded, FieldInfo field, Type parentClass, string path, bool isAsset)
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
            return ValidatorFailure.Of(this).Reason(logBuilder.ToString()).By(path, field.Name);
        }

        private List<ValidatorFailure> GuardRecursively(object behaviour, UnityEngine.Object parentObj, string path)
        {
            var failures = new List<ValidatorFailure>();
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
    }
}