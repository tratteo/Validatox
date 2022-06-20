using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    [Serializable]
    public abstract class Validator : ScriptableObject
    {
        [SerializeField, HideInInspector] private ValidationResult result;
        [SerializeField, HideInInspector] private bool hasResult;
        [SerializeField] private bool dirtyResult;
        [SerializeField] private byte[] shaChecksum;
        private SHA256 sha;

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
            SerializeValidation(this);
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
            SerializeValidation(this);
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
        { }

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
        protected virtual void SerializeValidation(Validator dirty) => AssetDatabase.SaveAssetIfDirty(dirty);

        private void OnValidate()
        {
            AssetDatabase.SaveAssetIfDirty(this);
            sha ??= SHA256.Create();
            var currentRepr = sha.ComputeHash(File.ReadAllBytes(AssetDatabase.GetAssetPath(this)));
            if (shaChecksum is null || shaChecksum.Length <= 0)
            {
                shaChecksum = currentRepr;
            }
            if (hasResult && !Enumerable.SequenceEqual(currentRepr, shaChecksum))
            {
                dirtyResult = true;
                shaChecksum = currentRepr;
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }
    }
}