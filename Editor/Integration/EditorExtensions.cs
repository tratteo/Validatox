using System;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    public static class EditorExtensions
    {
        public static bool IsNullOrDefault(this object arg)
        {
            if (arg is UnityEngine.Object && !(arg as UnityEngine.Object)) return true;

            if (arg is null) return true;

            if (Equals(arg, default)) return true;

            var methodType = arg.GetType();
            if (Nullable.GetUnderlyingType(methodType) is not null) return false;

            var argumentType = arg.GetType();
            if (argumentType.IsValueType && argumentType != methodType)
            {
                var obj = Activator.CreateInstance(arg.GetType());
                return obj.Equals(arg);
            }
            return false;
        }

        /// <summary>
        ///   Create and manage a property field
        /// </summary>
        /// <param name="editor"> </param>
        /// <param name="propName"> </param>
        /// <param name="label"> </param>
        /// <param name="tooltip"> </param>
        /// <returns> </returns>
        public static SerializedProperty PropertyField(this UnityEditor.Editor editor, string propName, string label = null, string tooltip = null)
        {
            var prop = editor.serializedObject.FindProperty(propName);
            label ??= propName;
            tooltip ??= string.Empty;
            EditorGUILayout.PropertyField(prop, new GUIContent(label, tooltip));
            return prop;
        }
    }
}