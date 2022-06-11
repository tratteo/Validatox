using System;
using System.Collections.Generic;

namespace Validatox.Editor.Validators
{
    public interface IValidable
    {
        public List<ValidatorFailure> Validate(Action<ValidationProgress> progress = null);

        public struct ValidationProgress
        {
            public string phase;

            public string description;

            public float value;

            public ValidationProgress(string phase, string description, float value)
            {
                this.phase = phase;
                this.value = value;
                this.description = description;
            }
        }
    }
}