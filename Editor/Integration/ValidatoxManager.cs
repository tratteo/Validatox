using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    public static class ValidatoxManager
    {
        public const string PackagePath = "Packages/com.siamango.validatox";
        public const string PackageEditorPath = "Packages/com.siamango.validatox/Editor";

        /// <summary>
        ///   Validate all <see cref="GroupValidator"/> in the <i> Asset </i> folder and sub-folders
        /// </summary>
        public static bool ValidateAllGroups()
        {
            static void Progress(ValidationProgress p) => EditorUtility.DisplayProgressBar(p.Phase, p.Description, p.ProgressValue);
            var res = ValidateAllGroups(Progress);
            EditorUtility.ClearProgressBar();
            return res;
        }

        /// <summary>
        ///   <inheritdoc cref="ValidateAllGroups"/>
        /// </summary>
        public static bool ValidateAllGroups(Action<ValidationProgress> progress)
        {
            var progressVal = new ValidationProgress(nameof(ValidatoxManager), "Retrieving groups...", 0);
            progress?.Invoke(progressVal);
            var objs = ValidatoxTools.GetAllBehavioursInAsset<GroupValidator>();
            var failure = false;
            for (var i = 0; i < objs.Count; i++)
            {
                objs[i].Validate(progress);
            }
            foreach (var obj in objs)
            {
                obj.LogResult();
            }
            return !failure;
        }

        /// <summary>
        ///   Validate all the fields marked with the <see cref="Meta.GuardedAttribute"/> using the default <see cref="GuardedValidator"/>
        ///   embedded in GibFrame. In order to override the default validator, create a new <see cref="GuardedValidator"/> in the <i>
        ///   Asset/Editor </i> folder of the project.
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
            var failure = false;
            var validatorPath = AssetDatabase.GetAssetPath(guardValidator);

            guardValidator.Validate(progress);
            AssetDatabase.LoadAssetAtPath<GuardValidator>(validatorPath).LogResult();
            return !failure;
        }

        public static GuardValidator LoadActiveGuard()
        {
            var settings = ValidatoxSettings.Load();
            if (string.IsNullOrEmpty(settings.GuardValidatorGuid))
            {
                var res = ValidatoxTools.GetAllBehavioursAtPath<GuardValidator>(PackageEditorPath);
                if (res.Count <= 0)
                {
                    ValidatoxLogEditorWindow.NotifyLog($"Unable to find {nameof(GuardValidator)}. Creating a new one in the package root folder", LogType.Warning);
                    var guard = ScriptableObject.CreateInstance<GuardValidator>();
                    AssetDatabase.CreateAsset(guard, $"{PackageEditorPath}{Path.AltDirectorySeparatorChar}ox_guard_validator.asset");
                    return guard;
                }
                return res[0];
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<GuardValidator>(AssetDatabase.GUIDToAssetPath(settings.GuardValidatorGuid));
            }
        }

        /// <summary>
        ///   Validate everything that is validable <b> :D </b>
        /// </summary>
        /// <returns> </returns>
        public static bool Validate()
        {
            static void Progress(ValidationProgress p) => EditorUtility.DisplayProgressBar(p.Phase, p.Description, p.ProgressValue);
            var groups = ValidateAllGroups(Progress);
            var guardeds = ValidateGuarded(Progress);
            EditorUtility.ClearProgressBar();
            return groups && guardeds;
        }
    }
}