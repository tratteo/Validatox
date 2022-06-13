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
        [SerializeField] private bool dirtyResult;

        public bool DirtyResult => dirtyResult;

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
            dirtyResult = false;
            SerializeIfDirty(this);
        }

        public ValidationResult Validate(Action<ValidationProgress> progress = null)
        {
            result = null;
            hasResult = false;
            dirtyResult = false;
            var res = new List<ValidationFailure>();
            if (!CanValidate()) return new ValidationResult(res, DateTime.Now);
            ValidationStart();
            Validate(res, progress);
            SerializeIfDirty(this);
            ValidationEnd();
            hasResult = true;
            result = new ValidationResult(res, DateTime.Now);
            return result;
        }

        public abstract void Validate(List<ValidationFailure> failures, Action<ValidationProgress> progress = null);

        /// <summary>
        ///   Called when the validation process is started
        /// </summary>
        protected virtual void ValidationStart()
        {
        }

        /// <summary>
        /// </summary>
        /// <returns> Whether the <see cref="Validator"/> can run the validation process </returns>
        protected virtual bool CanValidate() => true;

        /// <summary>
        ///   Called when the validation process is ended
        /// </summary>
        protected virtual void ValidationEnd()
        { }

        /// <summary>
        ///   Called when the <see cref="Validator"/> needs to be serialized
        /// </summary>
        protected virtual void SerializeIfDirty(Validator dirty)
        {
            AssetDatabase.SaveAssetIfDirty(dirty);
        }

        private void OnValidate()
        {
            if (hasResult) dirtyResult = true;
        }
    }
}