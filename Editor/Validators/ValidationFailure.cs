using System.Text;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Extern;
using Validatox.Editor.Validators.Fix;

namespace Validatox.Editor.Validators
{
    [System.Serializable]
    public class ValidationFailure
    {
        [SerializeField] private Validator validator;
        [SerializeField] private int code;
        [SerializeField] private string reason;
        [SerializeField] private string causerInstanceId;
        [SerializeField] private SerializableType fixType;
        [SerializeField] private object[] fixArgs;
        private GlobalObjectId? causerGlobalId;

        public Validator Validator => validator;

        public SerializableType FixType => fixType;

        public int Code => code;

        public string Reason => reason;

        private ValidationFailure(Validator validator, int causer)
        {
            this.validator = validator;
            code = 0;
            reason = string.Empty;
            causerInstanceId = GlobalObjectId.GetGlobalObjectIdSlow(causer).ToString();
            fixType = null;
        }

        public static ValidatorFailureBuilder Of(Validator validator, UnityEngine.Object causer)
        {
            GlobalObjectId.GetGlobalObjectIdSlow(causer.GetInstanceID());
            return new ValidatorFailureBuilder(validator, causer.GetInstanceID());
        }

        public ValidationFix InstantiateFix() => ValidationFix.InstantiateFix(FixType.GetType(), validator, GetCauserObject(), fixArgs ?? new object[] { });

        public UnityEngine.Object GetCauserObject()
        {
            if (causerGlobalId == null)
            {
                if (!GlobalObjectId.TryParse(causerInstanceId, out var id)) return null;
                causerGlobalId = id;
            }
            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow((GlobalObjectId)causerGlobalId);
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"{Validator} failure [{Code}]: {Reason}");
            var causer = GetCauserObject();
            if (causer != null)
            {
                if (causer.name.Length > 0) builder.Append(". Caused by ").Append(causer.name);
            }
            return builder.ToString();
        }

        public class ValidatorFailureBuilder
        {
            private readonly ValidationFailure failure;

            public ValidatorFailureBuilder(Validator validator, int causer)
            {
                failure = new ValidationFailure(validator, causer);
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

            public ValidatorFailureBuilder WithFix<T>(params object[] args) where T : ValidationFix
            {
                failure.fixType = typeof(T);
                failure.fixArgs = args;
                return this;
            }

            public ValidationFailure Build() => failure;
        }
    }
}