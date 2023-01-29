using UnityEditor;

namespace Validatox.Editor.Validators.Fix
{
    public class RenameFix : ValidationFix
    {
        private string newName;

        public RenameFix(Validator validator, UnityEngine.Object subject, params object[] args) : base(validator, subject, args)
        {
            newName = subject.name;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            EditorGUI.BeginChangeCheck();
            newName = EditorGUILayout.TextField(newName);
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Subject), newName);
                Apply();
            }
        }
    }
}