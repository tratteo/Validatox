using UnityEditor;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    [CustomEditor(typeof(ValidationGroup))]
    public class ValidatorGroupCustomEditor : ValidatorCustomEditor
    {
        protected override void DrawProperties()
        {
            this.PropertyField("validators", "Validators");
        }
    }
}