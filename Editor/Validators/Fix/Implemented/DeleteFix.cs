using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class DeleteFix : ValidationFix
    {
        public DeleteFix(Validator validator, Object subject, params object[] args) : base(validator, subject, args)
        {
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            if (GUILayout.Button("Delete"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Subject));
                Validator.MarkDirtyValidation();
                window.Close();
            }
        }
    }
}