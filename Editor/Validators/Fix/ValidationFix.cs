using System;

using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public abstract class ValidationFix
    {
        public enum OpenStyle
        { Popup, Utility, Normal }

        public OpenStyle Style { get; protected set; } = OpenStyle.Popup;

        public string Title { get; protected set; }

        public Vector2 Size { get; protected set; } = new Vector2(400, 150);

        protected UnityEngine.Object Subject { get; private set; }

        protected Validator Validator { get; private set; }

        protected SerializedObject SerializedObject { get; private set; }

        protected object[] Args { get; private set; }

        protected ValidationFix(Validator validator, UnityEngine.Object subject, params object[] args)
        {
            Validator = validator;
            Args = args;
            Subject = subject;
            Title = $"Fix for {subject.name}";
            SerializedObject = new SerializedObject(subject);
        }

        public static ValidationFix InstantiateFix(Type type, Validator validator, UnityEngine.Object causer, params object[] args) =>
            (ValidationFix)Activator.CreateInstance(type, new object[] { validator, causer, args });

        public abstract void EditorRender(ValidationFixWindow window);

        protected void ErrorLabel(string error)
        {
            EditorGUILayout.LabelField($"The fix cannot be applied: {error}",
                EditorStyles.wordWrappedLabel.Copy(s => s.normal.textColor = Color.red));
        }

        /// <summary>
        ///   Apply the modifications to the serialized object
        /// </summary>
        protected void Apply()
        {
            if (SerializedObject.hasModifiedProperties)
            {
                SerializedObject.ApplyModifiedProperties();
                Validator.MarkDirtyValidation();
            }
        }
    }
}