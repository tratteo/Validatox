using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Add component fix. Eligible to automatic fixing
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class AddComponentFix<T> : ValidationFix where T : Component
    {
        public AddComponentFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            ContextlessFix = true;
        }

        public override string GetLabel() => $"{nameof(AddComponentFix<T>)}<{typeof(T).Name}>";

        protected override bool Fix(SerializedObject serializedObject)
        {
            var obj = serializedObject.targetObject;
            if (obj is GameObject gObj)
            {
                gObj.AddComponent<T>();
                return true;
            }
            return false;
        }
    }
}