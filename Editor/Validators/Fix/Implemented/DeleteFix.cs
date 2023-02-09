using UnityEditor;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Delete an object fix. Eligible to automatic fixing
    /// </summary>
    public class DeleteFix : ValidationFix
    {
        public DeleteFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            ContextlessFix = true;
        }

        protected override bool Fix(SerializedObject serializedObject)
        {
            if (IsSceneObject)
            {
                UnityEngine.Object.DestroyImmediate(serializedObject.targetObject);
            }
            else
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(serializedObject.targetObject));
            }
            return true;
        }
    }
}