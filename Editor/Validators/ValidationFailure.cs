using System.Linq;
using System.Text;
using UnityEngine;

namespace Validatox.Editor.Validators
{
    [System.Serializable]
    public class ValidationFailure
    {
        [SerializeField] private Validator validator;
        [SerializeField] private int code;
        [SerializeField] private string reason;
        [SerializeField] private string[] causersNames;

        public Validator Validator => validator;

        public int Code => code;

        public string Reason => reason;

        public string[] Causers => causersNames;

        private ValidationFailure(Validator validator)
        {
            this.validator = validator;
            code = 0;
            reason = string.Empty;
            causersNames = new string[0];
        }

        public static ValidatorFailureBuilder Of(Validator validator) => new ValidatorFailureBuilder(validator);

        public override string ToString()
        {
            var builder = new StringBuilder($"{Validator} failure [{Code}]: {Reason}");
            if (Causers.Length > 0) builder.Append(". Caused by ");
            for (var i = 0; i < Causers.Length; i++)
            {
                var o = Causers[i];

                builder.Append(o);
                if (i < Causers.Length - 1)
                {
                    builder.Append(" | ");
                }
            }
            return builder.ToString();
        }

        public class ValidatorFailureBuilder
        {
            private readonly ValidationFailure failure;

            public ValidatorFailureBuilder(Validator validator)
            {
                failure = new ValidationFailure(validator);
            }

            public static implicit operator ValidationFailure(ValidatorFailureBuilder builder) => builder.Build();

            public ValidatorFailureBuilder Reason(string reason)
            {
                failure.reason = reason;
                return this;
            }

            public ValidatorFailureBuilder Code(int code)
            {
                failure.code = code;
                return this;
            }

            public ValidatorFailureBuilder By(params object[] causers)
            {
                failure.causersNames = (from c in causers select c.ToString()).ToArray();
                return this;
            }

            public ValidationFailure Build() => failure;
        }
    }
}