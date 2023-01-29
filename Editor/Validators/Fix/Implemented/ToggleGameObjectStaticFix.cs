using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class ToggleGameObjectStaticFix : ValidationFix
    {
        private readonly GameObject gObj;
        private bool isStatic;

        public ToggleGameObjectStaticFix(Validator validator, UnityEngine.Object subject, params object[] args) : base(validator, subject, args)
        {
            gObj = subject is GameObject ? subject as GameObject : null;
            isStatic = gObj != null && gObj.isStatic;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            if (gObj == null)
            {
                ErrorLabel("the subject is not a GameObject");
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                isStatic = EditorGUILayout.Toggle("Static", isStatic);
                if (EditorGUI.EndChangeCheck())
                {
                    gObj.isStatic = isStatic;
                    Apply();
                }
            }
        }
    }
}