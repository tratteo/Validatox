using System;
using System.Collections.Generic;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    /// <summary>
    ///   A collection of <see cref="Validator"/> elements that can be validated in the Editor
    /// </summary>
    [CreateAssetMenu(menuName = "Validatox/Validation group", fileName = "validation_group")]
    public sealed class ValidationGroup : ScriptableObject, IValidable
    {
        [SerializeField] private List<Validator> validators;
        [SerializeField, HideInInspector] private bool showResults;
        [SerializeField, HideInInspector] private List<string> latestResultsStrings;
        [SerializeField, HideInInspector] private string lastValidationTime = string.Empty;

        public IReadOnlyCollection<Validator> Validators => validators;

        public List<ValidatorFailure> Validate(Action<IValidable.ValidationProgress> progress = null)
        {
            var failures = new List<ValidatorFailure>();
            if (validators is null || validators.Count < 0) return failures;
            var progressVal = new IValidable.ValidationProgress(name, "Validating...", 0);
            progress?.Invoke(progressVal);
            for (var i = 0; i < validators.Count; i++)
            {
                var v = validators[i];
                if (!v) continue;
                progressVal.value = (float)i / validators.Count;
                progress?.Invoke(progressVal);
                var res = v.Validate(progress);
                if (res is not null && res.Count > 0)
                {
                    failures.AddRange(res);
                }
            }
            latestResultsStrings = failures.ConvertAll(s => s.ToString());
            lastValidationTime = DateTime.Now.ToString();
            showResults = true;
            return failures;
        }
    }
}