using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class RemoveComponentFix<T> : ValidationFix where T : Component
    {
        private readonly GameObject gObj;

        public RemoveComponentFix(Validator validator, UnityEngine.Object subject, params object[] args) : base(validator, subject, args)
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
                if (GUILayout.Button($"Remove {typeof(T)}"))
                {
                    if (gObj.TryGetComponent<T>(out var comp))
                    {
                        UnityEngine.Object.DestroyImmediate(comp, true);
                        Validator.MarkDirtyValidation();
                    }
                }
            }
        }
    }
}