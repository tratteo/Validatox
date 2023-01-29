using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Requires the first param to be a string indicating the name of the field
    /// </summary>
    public class MissingReferenceFix : ValidationFix
    {
        private readonly SerializedProperty prop;

        private readonly string fieldName = string.Empty;

        public MissingReferenceFix(Validator validator, Object subject, params object[] args) : base(validator, subject, args)
        {
            if (args.Length > 0 && args[0] is string val)
            {
                fieldName = val;
            }
            prop = SerializedObject.FindProperty(fieldName);
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            if (prop == null || SerializedObject == null)
            {
                ErrorLabel("serialized property is missing. Try to run validation again");
            }
            else
            {
                EditorGUILayout.PropertyField(prop, new GUIContent(fieldName));
                Apply();
            }
        }
    }
}