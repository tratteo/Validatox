﻿using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    public static class ValidatoxManager
    {
        /// <summary>
        ///   Validate all the fields marked with the <see cref="Meta.GuardAttribute"/> using the default <see cref="GuardValidator"/>
        ///   embedded in GibFrame. In order to override the default validator, create a new <see cref="GuardValidator"/>.
        /// </summary>
        public static bool ValidateGuarded()
        {
            static void Progress(ValidationProgress p) => EditorUtility.DisplayProgressBar(p.Phase, p.Description, p.ProgressValue);
            var res = ValidateGuarded(Progress);
            EditorUtility.ClearProgressBar();
            return res;
        }

        /// <summary>
        ///   <inheritdoc cref="ValidateGuarded"/>
        /// </summary>
        public static bool ValidateGuarded(Action<ValidationProgress> progress)
        {
            var guardValidator = LoadActiveGuard();
            var validatorPath = AssetDatabase.GetAssetPath(guardValidator);

            var res = guardValidator.Validate(progress);
            AssetDatabase.LoadAssetAtPath<GuardValidator>(validatorPath).LogResult();
            return res.Successful;
        }

        /// <summary>
        ///   Load the currently active <see cref="GuardValidator"/>
        /// </summary>
        /// <returns> </returns>
        public static GuardValidator LoadActiveGuard()
        {
            var settings = ValidatoxSettings.Load();
            var loaded = AssetDatabase.LoadAssetAtPath<GuardValidator>(AssetDatabase.GUIDToAssetPath(settings.GuardValidatorGuid));
            if (loaded)
            {
                return loaded;
            }
            else
            {
                var res = ValidatoxTools.GetAllBehavioursInAsset<GuardValidator>();
                if (res.Count <= 0)
                {
                    ValidatoxLogEditorWindow.Notify(Log.Create($"Unable to find {nameof(GuardValidator)}. Creating a new one in the package root folder").Type(LogType.Warning));
                    var val = ScriptableObject.CreateInstance<GuardValidator>();
                    Directory.CreateDirectory($"Assets{Path.AltDirectorySeparatorChar}{Resources.Author}{Path.AltDirectorySeparatorChar}{Resources.Name}");
                    AssetDatabase.CreateAsset(val, $"Assets{Path.AltDirectorySeparatorChar}{Resources.Author}{Path.AltDirectorySeparatorChar}{Resources.Name}{Path.AltDirectorySeparatorChar}ox_guard_validator.asset");
                    settings.GuardValidatorGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(val)).ToString();
                    ValidatoxSettings.Save(settings);
                    return val;
                }
                settings.GuardValidatorGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(res[0])).ToString();
                ValidatoxSettings.Save(settings);
                return res[0];
            }
        }

        /// <summary>
        ///   <inheritdoc cref="ValidateGroupsAndSingles(Action{ValidationProgress})"/>
        /// </summary>
        /// <returns> </returns>
        public static bool ValidateGroupsAndSingles()
        {
            static void Progress(ValidationProgress p) => EditorUtility.DisplayProgressBar(p.Phase, p.Description, p.ProgressValue);
            var res = ValidateGroupsAndSingles(Progress);
            EditorUtility.ClearProgressBar();
            return res;
        }

        /// <summary>
        ///   Validate all <see cref="Validator"/> and <see cref="GroupValidator"/>. Automatically prevent validators to be validated twice.
        /// </summary>
        /// <param name="progress"> </param>
        /// <returns> </returns>
        public static bool ValidateGroupsAndSingles(Action<ValidationProgress> progress)
        {
            var progressVal = new ValidationProgress(nameof(ValidatoxManager), "Retrieving validators...", 0);
            progress?.Invoke(progressVal);
            var objs = ValidatoxTools.GetAllBehavioursInAsset<Validator>(string.Empty, null, typeof(GuardValidator));
            var gropus = objs.FindAll(v => v is GroupValidator).ConvertAll(v => v as GroupValidator);
            objs.RemoveAll(v => gropus.Contains(v));
            var failure = false;
            for (var i = 0; i < gropus.Count; i++)
            {
                var res = gropus[i].Validate(progress);
                if (!res.Successful) failure = true;
                var vals = gropus[i].GetValidators();
                objs.RemoveAll(v => vals.Contains(v));
            }

            for (var i = 0; i < objs.Count; i++)
            {
                var res = objs[i].Validate(progress);
                if (!res.Successful) failure = true;
            }
            foreach (var obj in gropus)
            {
                obj.LogResult();
            }
            foreach (var obj in objs)
            {
                obj.LogResult();
            }
            return !failure;
        }

        /// <summary>
        ///   Validate everything that is validable <b> :D </b>
        /// </summary>
        /// <returns> </returns>
        public static bool Validate()
        {
            static void Progress(ValidationProgress p) => EditorUtility.DisplayProgressBar(p.Phase, p.Description, p.ProgressValue);
            var guardeds = ValidateGuarded(Progress);
            var groups = ValidateGroupsAndSingles(Progress);
            EditorUtility.ClearProgressBar();
            return groups && guardeds;
        }
    }
}