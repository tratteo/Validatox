using UnityEditor;
using UnityEngine;
using Validatox.Meta;

namespace Validatox.Editor
{
    [CustomPropertyDrawer(typeof(GuardAttribute))]
    internal class GuardAttributePropertyDrawer : PropertyDrawer
    {
        private bool error = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property, label);
            var rect = position;
            rect.y += EditorGUIUtility.singleLineHeight;
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                error = true;
                var initColor = GUI.color;
                GUI.color = Color.red;
                EditorGUI.LabelField(rect, "Not a reference value, will not be analyzed by the GuardValidator");
                GUI.color = initColor;
            }
            else
            {
                error = false;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => error ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight;
    }
}