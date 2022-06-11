﻿using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    public static class Validatox
    {
        public const string PackagePath = "Packages/com.tratteo.validatox";
        public const string PackageEditorPath = "Packages/com.tratteo.validatox/Editor";

        /// <summary>
        ///   Validate all <see cref="ValidationGroup"/> in the <i> Asset </i> folder and sub-folders
        /// </summary>
        public static bool ValidateAllGroups()
        {
            static void Progress(IValidable.ValidationProgress p) => EditorUtility.DisplayProgressBar(p.phase, p.description, p.value);
            var res = ValidateAllGroups(Progress);
            EditorUtility.ClearProgressBar();
            return res;
        }

        /// <summary>
        ///   <inheritdoc cref="ValidateAllGroups"/>
        /// </summary>
        public static bool ValidateAllGroups(Action<IValidable.ValidationProgress> progress)
        {
            var progressVal = new IValidable.ValidationProgress(nameof(Validatox), "Retrieving groups...", 0);
            progress?.Invoke(progressVal);
            var objs = ValidatoxTools.GetAllBehavioursInAsset<ValidationGroup>();
            var failure = false;
            for (var i = 0; i < objs.Count; i++)
            {
                var o = objs[i];
                var res = o.Validate(progress);
                if (res.Count <= 0)
                {
                    Debug.Log($"{o.name} -> <color=#55d971>Validation successful! <b>:D</b></color>", o);
                }
                else
                {
                    failure = true;
                    var builder = new StringBuilder($"{o.name} -> <color=#ed4e4e>Validation failed with {res.Count} errors</color>\nClick for more info\n");
                    foreach (var r in res)
                    {
                        builder.Append(r.ToString() + "\n");
                    }
                    Debug.LogError(builder.ToString(), o);
                }
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
            static void Progress(IValidable.ValidationProgress p) => EditorUtility.DisplayProgressBar(p.phase, p.description, p.value);
            var res = ValidateGuarded(Progress);
            EditorUtility.ClearProgressBar();
            return res;
        }

        /// <summary>
        ///   <inheritdoc cref="ValidateGuarded"/>
        /// </summary>
        public static bool ValidateGuarded(Action<IValidable.ValidationProgress> progress)
        {
            var res = ValidatoxTools.GetAllBehavioursInAsset<GuardValidator>($"Editor");
            if (res.Count > 0)
            {
                Debug.Log($"Using override of {nameof(GuardValidator)}");
            }
            else
            {
                res = ValidatoxTools.GetAllBehavioursAtPath<GuardValidator>(PackageEditorPath);
                if (res.Count <= 0)
                {
                    Debug.LogWarning($"Unable to find {nameof(GuardValidator)}. Try creating a new one in the Editor folder of the project");
                    return false;
                }
            }
            var validator = res[0];
            var failure = false;
            var validatorName = validator.name;
            var failures = validator.Validate(progress);
            if (failures.Count <= 0)
            {
                Debug.Log($"{validatorName} -> <color=#55d971>Validation successful! <b>:D</b></color>");
            }
            else
            {
                failure = true;
                var builder = new StringBuilder($"{validatorName} -> <color=#ed4e4e>Validation failed with {failures.Count} errors</color>\nClick for more info\n");
                foreach (var r in failures)
                {
                    builder.Append(r.ToString() + "\n");
                }
                Debug.LogError(builder.ToString());
            }

            return !failure;
        }

        /// <summary>
        ///   Validate everything that is validable <b> :D </b>
        /// </summary>
        /// <returns> </returns>
        public static bool Validate()
        {
            static void Progress(IValidable.ValidationProgress p) => EditorUtility.DisplayProgressBar(p.phase, p.description, p.value);
            var groups = ValidateAllGroups(Progress);
            var guardeds = ValidateGuarded(Progress);
            EditorUtility.ClearProgressBar();
            return groups && guardeds;
        }
    }
}