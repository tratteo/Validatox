using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Validators.Fix;

namespace Validatox.Editor.Validators
{
    [Serializable]
    public abstract class Validator : ScriptableObject
    {
        [SerializeField, HideInInspector] private ValidationResult result;
        [SerializeField, HideInInspector] private bool hasResult;
        [SerializeField] private bool dirtyResult;

        /// <summary>
        ///   Whether the validator has a result ready
        /// </summary>
        public bool HasResult => hasResult;

        /// <summary>
        ///   Whether the validator may be changed since last validation
        /// </summary>
        public bool DirtyResult => dirtyResult;

        /// <summary>
        ///   Try to get the cached <see cref="ValidationResult"/>
        /// </summary>
        /// <param name="result"> </param>
        /// <returns> </returns>
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

        /// <summary>
        ///   Clear the <see cref="ValidationResult"/> of the validator
        /// </summary>
        public void ClearResult()
        {
            result = null;
            hasResult = false;
            dirtyResult = false;
            ForceSerialize(this);
        }

        /// <summary>
        ///   Mark the latest <see cref="ValidationResult"/> as dirty
        /// </summary>
        public void MarkDirtyValidation() => dirtyResult = true;

        /// <summary>
        ///   Attempt to apply all <see cref="ValidationFix"/> related to all the <see cref="ValidationFailure"/> in the cached <see cref="ValidationResult"/>
        /// </summary>
        /// <returns> </returns>
        public List<ValidationFix> ApplyAutomaticFixes()
        {
            var fixes = new List<ValidationFix>();
            if (hasResult && result.Failures != null)
            {
                foreach (var f in result.Failures)
                {
                    if (f.TryGetFix(out var fix) && fix.ContextlessFix)
                    {
                        fix.ApplyFix();
                        fixes.Add(fix);
                    }
                }
            }
            return fixes;
        }

        /// <summary>
        ///   Log the result if present. Use <see cref="ValidatoxLogEditorWindow.Notify(IEnumerable{ValidationFailure}, Log)"/> for logging failures
        /// </summary>
        public virtual void LogResult()
        {
            if (!hasResult) return;
            if (result.Successful)
            {
                ValidatoxLogEditorWindow.Notify(Log.Create($"{name} > Validation successful!").Type(LogType.Assert));
            }
            else
            {
                ValidatoxLogEditorWindow.Notify(result.Failures,
                    Log.Create($"{name} Validation failed with <b>{result.Failures.Count}</b> errors").Type(LogType.Error).WithSubject(this));
            }
        }

        /// <summary>
        ///   Attempt to perform a single validation. If the validator does not have a result, execute a total validation
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="failure"> </param>
        public void PerformSingleValidation(UnityEngine.Object obj, ValidationFailure failure)
        {
            if (!hasResult)
            {
                Validate(EditorProgressReport);
                EditorUtility.ClearProgressBar();
            }
            else
            {
                var newFailures = ValidateSingle(obj, failure.ScenePath);
                var oldFailures = result.Failures.ToList();
                oldFailures.RemoveAll(f => f.GetId() == failure.GetId());
                oldFailures.AddRange(newFailures);
                hasResult = true;
                result = new ValidationResult(oldFailures, DateTime.Now);
                ForceSerialize(this);
            }
        }

        /// <summary>
        ///   Run the validation for the current validator
        /// </summary>
        /// <param name="progress"> </param>
        /// <returns> </returns>
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
        ///   Run the validation on a single object. The parameter <paramref name="scenePath"/> is not null in case the object is in a scene
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="scenePath"> </param>
        /// <returns> </returns>
        protected virtual List<ValidationFailure> ValidateSingle(UnityEngine.Object obj, string scenePath)
        { return new List<ValidationFailure>(); }

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

        private void EditorProgressReport(ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue);
    }
}