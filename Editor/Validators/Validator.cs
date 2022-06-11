using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    /// <summary>
    ///   Extend this base class to create any kind of validator <see cref="ScriptableObject"/> able to be validated
    /// </summary>
    public abstract class Validator : ScriptableObject
    {
        [SerializeField, HideInInspector] private ValidationResult result;
        [SerializeField, HideInInspector] private bool hasResult;

        public bool TryGetCachedResult(out ValidationResult result)
        {
            if (hasResult)
            {
                result = this.result;
                return true;
            }
            result = null;
            return false;
        }

        public void ClearResult()
        {
            result = null;
            hasResult = false;
            EditorUtility.SetDirty(this);
        }

        public ValidationResult Validate(Action<ValidationProgress> progress = null)
        {
            var res = new List<ValidationFailure>();
            Validate(res, progress);
            result = new ValidationResult(res, DateTime.Now);
            hasResult = true;
            EditorUtility.SetDirty(this);
            return result;
        }

        public abstract void Validate(List<ValidationFailure> failures, Action<ValidationProgress> progress = null);
    }
}