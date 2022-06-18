using System;
using System.Collections.Generic;
using UnityEngine;
using Validatox.Meta;

namespace Validatox.Editor.Validators
{
    [Serializable]
    /// <summary>
    ///   A collection of <see cref="Validator"/> elements that can be validated in the Editor
    /// </summary>
    [CreateAssetMenu(menuName = "Validatox/Validation group", fileName = "validation_group")]
    public class GroupValidator : Validator
    {
        [ExcludeSubtypes(typeof(GroupValidator), ExcludeSubclasses = true)]
        [SerializeField] private List<Validator> validators;

        public IReadOnlyCollection<Validator> GetValidators() => validators is null ? new List<Validator>() : validators.FindAll(v => v);

        public override void Validate(List<ValidationFailure> failures, Action<ValidationProgress> progress = null)
        {
            if (validators is null || validators.Count < 0) return;
            var progressVal = new ValidationProgress(name, "Validating...");
            progress?.Invoke(progressVal);
            for (var i = 0; i < validators.Count; i++)
            {
                var v = validators[i];
                if (!v) continue;
                var res = v.Validate(p =>
                {
                    progress?.Invoke(p.At(name).Doing(v.name + ": " + p.Description).WithProgress((i + p.ProgressValue) / validators.Count));
                });
                if (res is not null && res.Failures.Count > 0)
                {
                    failures.AddRange(res.Failures);
                }
            }
        }
    }
}