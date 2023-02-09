using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Validatox.Editor.Extern;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Fix a missing reference to any <see cref="UnityEngine.Object"/>. Default fix used by the <see cref="GuardValidator"/>. Required
    ///   the first parameter to be the field name that corresponds to the missing reference
    /// </summary>
    public class MissingReferenceFix : ValidationFix
    {
        private readonly string fieldName = string.Empty;
        private readonly SerializableType fieldType;
        private UnityEngine.Object reference;

        public MissingReferenceFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            Size = new Vector2(400, 150);
            if (args.Length > 1 && args[0] is string val && args[1] is SerializableType t)
            {
                fieldName = val;
                fieldType = t;
            }
        }

        protected override bool Fix(SerializedObject serializedObject)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = reference;
                return reference;
            }
            else
            {
                Debug.LogWarning("unable to find serialized property");
                return false;
            }
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            var activeScene = SceneManager.GetActiveScene();
            Type type;
            if (fieldType == null || (type = fieldType.GetType()) == null)
            {
                ErrorLabel("missing field type, try to run validation again");
            }
            else
            {
                reference = EditorGUILayout.ObjectField(reference, type, activeScene.path == Failure.ScenePath);
            }
        }
    }
}