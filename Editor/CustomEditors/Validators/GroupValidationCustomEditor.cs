using UnityEditor;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    [CustomEditor(typeof(GroupValidator), true)]
    public class GroupValidatorCustomEditor : ValidatorCustomEditor
    {
        protected override void DrawProperties()
        {
            this.PropertyField("validators", "Validators");
            base.DrawProperties();
        }
    }
}