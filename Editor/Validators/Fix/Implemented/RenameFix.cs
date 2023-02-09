using UnityEditor;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Rename an object. Eligible to automatic fixing if one argument is provided as the static name to apply
    /// </summary>
    public class RenameFix : ValidationFix
    {
        private string newName;
        private string contextlessName;

        public RenameFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            if (failure.TryGetSubject(out var obj))
            {
                newName = obj.name;
            }
            if (args.Length > 0 && args[0] is string name)
            {
                newName = name;
                contextlessName = name;
                ContextlessFix = true;
            }
        }

        public override string GetLabel() => ContextlessFix ? $"{nameof(RenameFix)} > {contextlessName}" : nameof(RenameFix);

        protected override bool Fix(SerializedObject serializedObject)
        {
            newName = contextlessName != null ? contextlessName : newName;
            if (IsSceneObject)
            {
                serializedObject.targetObject.name = newName;
            }
            else
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(serializedObject.targetObject), newName);
            }
            return true;
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            base.EditorRender(window);
            newName = EditorGUILayout.TextField("Rename", newName);
        }
    }
}