using UnityEditor;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Rename an object. Eligible to automatic fixing if one argument is provided as the static name to apply
    /// </summary>
    public class RenameFix : ValidationFix
    {
        private string newName;

        public RenameFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            if (failure.TryGetSubject(out var obj))
            {
                newName = obj.name;
            }
            if (args.Length > 0 && args[0] is string name)
            {
                newName = name;
                ContextlessFix = true;
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {newName}";
        }

        protected override bool Fix(SerializedObject serializedObject)
        {
            if (IsSceneObject)
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(serializedObject.targetObject), newName);
            }
            else
            {
                serializedObject.targetObject.name = newName;
            }
            return true;
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            EditorGUI.BeginChangeCheck();
            newName = EditorGUILayout.TextField("Rename", newName);
        }
    }
}