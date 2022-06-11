using System;
using System.Collections.Generic;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    [Serializable]
    public class ValidationResult
    {
        [SerializeField] private List<ValidationFailure> failures;
        [SerializeField] private string timeString;

        public DateTime Time => DateTime.Parse(timeString);

        public IReadOnlyCollection<ValidationFailure> Failures => failures;

        public bool Successful => failures.Count <= 0;

        public ValidationResult(List<ValidationFailure> failures, DateTime dateTime)
        {
            this.failures = failures is not null ? failures : new List<ValidationFailure>();
            timeString = dateTime.ToString();
        }
    }
}