using System;
using System.Collections.Generic;
using System.Text;
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
            ForceSerialize(this);
        }

        public void LogResult()
        {
            if (!hasResult) return;
            if (result.Successful)
            {
                ValidatoxLogEditorWindow.NotifyLog($"{name} -> <color=#55d971>Validation successful! <b>:D</b></color>", LogType.Assert, this);
            }
            else
            {
                var builder = new StringBuilder($"{name} -> <color=#ed4e4e>Validation failed with {result.Failures.Count} errors <b>X(</b> \n</color>");
                foreach (var r in result.Failures)
                {
                    builder.Append(r.ToString() + "\n");
                }
                ValidatoxLogEditorWindow.NotifyLog(builder.ToString(), LogType.Error, this);
            }
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
            hasResult = true;
            result = new ValidationResult(res, DateTime.Now);
            ForceSerialize(this);
            ValidationEnd();
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
        protected virtual void ForceSerialize(Validator dirty)
        {
            EditorUtility.SetDirty(dirty);
            AssetDatabase.SaveAssetIfDirty(dirty);
        }
    }
}