using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Remove a certain component fix. Eligible to automatic fixing
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class RemoveComponentFix<T> : ValidationFix where T : Component
    {
        public RemoveComponentFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            ContextlessFix = true;
        }

        public override string GetLabel() => $"{nameof(RemoveComponentFix<T>)}<{typeof(T).Name}>";

        protected override bool Fix(SerializedObject serializedObject)
        {
            var obj = serializedObject.targetObject;
            if (obj is GameObject gObj)
            {
                if (gObj.TryGetComponent<T>(out var comp))
                {
                    Object.DestroyImmediate(comp, true);
                    return true;
                }
            }
            return false;
        }
    }
}