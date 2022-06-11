using System;
using System.Collections.Generic;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    /// <summary>
    ///   Extend this base class to create any kind of validator <see cref="ScriptableObject"/> able to be validated
    /// </summary>
    public abstract class Validator : ScriptableObject, IValidable
    {
        [SerializeField, HideInInspector] private bool showResults;
        [SerializeField, HideInInspector] private List<string> latestResultsStrings;
        [SerializeField, HideInInspector] private string lastValidationTime = string.Empty;

        public bool IsBeingValidated { get; private set; }

        public List<ValidatorFailure> Validate(Action<IValidable.ValidationProgress> progress = null)
        {
            IsBeingValidated = true;
            var res = new List<ValidatorFailure>();
            Validate(res, progress);
            IsBeingValidated = false;
            latestResultsStrings = res.ConvertAll(s => s.ToString());
            lastValidationTime = DateTime.Now.ToString();
            showResults = true;
            return res;
        }

        public abstract void Validate(List<ValidatorFailure> failures, Action<IValidable.ValidationProgress> progress = null);
    }
}