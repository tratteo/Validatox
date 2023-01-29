using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class AddComponentFix<T> : ValidationFix where T : Component
    {
        private readonly GameObject gObj;

        public AddComponentFix(Validator validator, UnityEngine.Object subject, params object[] args) : base(validator, subject, args)
        {
            gObj = subject is GameObject ? subject as GameObject : null;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            if (gObj == null)
            {
                ErrorLabel("the subject is not a GameObject");
            }
            else
            {
                if (GUILayout.Button($"Add {typeof(T)}"))
                {
                    gObj.AddComponent<T>();
                    Apply();
                }
            }
        }
    }
}