using System;
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
        [SerializeField] private string guid;
        [SerializeField] private Validator validator;
        [SerializeField] private string reason;
        [SerializeField] private string causerInstanceId;
        [SerializeField] private SerializableType fixType;
        [SerializeField] private object[] fixArgs;
        [SerializeField] private string scenePath;
        private GlobalObjectId? causerGlobalId;
        private UnityEngine.Object cachedSubject;

        /// <summary>
        ///   The validator that caused this failure
        /// </summary>
        public Validator Validator => validator;

        /// <summary>
        ///   The serializable type of the <see cref="ValidationFix"/> for this failure
        /// </summary>
        public SerializableType FixType => fixType;

        /// <summary>
        ///   A nullable property that indicates the scene the subject of this failure belongs to
        /// </summary>
        public string ScenePath => scenePath;

        /// <summary>
        ///   The reason of the failure
        /// </summary>
        public string Reason => reason;

        private ValidationFailure(Validator validator)
        {
            guid = Guid.NewGuid().ToString();
            this.validator = validator;
            reason = string.Empty;
            causerInstanceId = null;
            fixType = null;
        }

        public static ValidatorFailureBuilder Of(Validator validator) => new ValidatorFailureBuilder(validator);

        /// <summary>
        ///   Get the unique <see cref="Guid"/> of this validation
        /// </summary>
        /// <returns> </returns>
        public string GetId()
        {
            if (Guid.TryParse(guid, out var id)) return id.ToString();
            var g = GUID.Generate();
            guid = g.ToString();
            return guid;
        }

        /// <summary>
        ///   Attempt to instantiate the fix of this failure
        /// </summary>
        /// <returns> </returns>
        public bool TryGetFix(out ValidationFix fix)
        {
            if (fixType == null || fixType.GetType() == null)
            {
                fix = null;

                return false;
            }
            fix = ValidationFix.InstantiateFix(FixType.GetType(), this, fixArgs ?? new object[] { });
            return true;
        }

        /// <summary>
        ///   Attempt to retrieve the subject of this failure. Note that if the subject is in a closed scene, this method will return null.
        /// </summary>
        /// <param name="obj"> </param>
        /// <returns> </returns>
        public bool TryGetSubject(out UnityEngine.Object obj)
        {
            if (cachedSubject)
            {
                obj = cachedSubject;
                return true;
            }
            obj = null;
            if (causerInstanceId == null) return false;

            if (causerGlobalId == null)
            {
                if (!GlobalObjectId.TryParse(causerInstanceId, out var id)) return false;
                causerGlobalId = id;
            }

            cachedSubject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow((GlobalObjectId)causerGlobalId);
            obj = cachedSubject;
            return cachedSubject;
        }

        /// <summary>
        ///   Get a representation of this failure, formatting the reason and the data
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            var builder = new StringBuilder($"Failure");
            if (!string.IsNullOrEmpty(Reason))
            {
                builder.Append($" {Reason}");
            }
            if (TryGetSubject(out var causer))
            {
                if (causer.name.Length > 0) builder.Append(" caused by <b>").Append(causer.name).Append("</b>");
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

            /// <summary>
            ///   Specify the reason of this failure
            /// </summary>
            /// <param name="reason"> </param>
            /// <returns> </returns>
            public ValidatorFailureBuilder Reason(string reason)
            {
                failure.reason = reason;
                return this;
            }

            /// <summary>
            ///   Identify the object that caused the failure
            /// </summary>
            /// <param name="subject"> </param>
            /// <param name="scenePath"> </param>
            /// <returns> </returns>
            public ValidatorFailureBuilder CausedBy(UnityEngine.Object subject, string scenePath = null)
            {
                failure.causerInstanceId = GlobalObjectId.GetGlobalObjectIdSlow(subject.GetInstanceID()).ToString();
                failure.scenePath = scenePath;
                return this;
            }

            /// <summary>
            ///   Specify a <see cref="ValidationFix"/> that can be used to fix this failure
            /// </summary>
            /// <typeparam name="T"> </typeparam>
            /// <param name="args"> </param>
            /// <returns> </returns>
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