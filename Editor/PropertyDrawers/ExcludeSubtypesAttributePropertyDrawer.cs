using UnityEditor;
using UnityEngine;
using Validatox.Meta;

namespace Validatox.Editor
{
    [CustomPropertyDrawer(typeof(ExcludeSubtypesAttribute))]
    internal class ExcludeSubtypesAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var uniqueAttribute = attribute as ExcludeSubtypesAttribute;
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                var errorStyle = new GUIStyle();
                errorStyle.normal.textColor = Color.red;
                EditorGUI.LabelField(position, label.text, $"Use [{nameof(ExcludeSubtypesAttribute)}] with references types.", errorStyle);
            }
            else
            {
                var oldVal = property.objectReferenceValue;
                EditorGUI.PropertyField(position, property, label, true);
                if (property.objectReferenceValue)
                {
                    if (!IsPropertyValueValid(uniqueAttribute, property.objectReferenceValue))
                    {
                        Debug.LogWarning($"Type {property.objectReferenceValue.GetType().FullName} is excluded from field {property.name} [{nameof(ExcludeSubtypesAttribute)}]");
                        property.serializedObject.Update();
                        property.objectReferenceValue = !oldVal ? null : IsPropertyValueValid(uniqueAttribute, oldVal) ? oldVal : null;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        private bool IsPropertyValueValid(ExcludeSubtypesAttribute attribute, UnityEngine.Object value)
        {
            var valueType = value.GetType();
            foreach (var type in attribute.ExcludedTypes)
            {
                if (valueType == type) return false;
                if (attribute.ExcludeSubclasses && type.IsAssignableFrom(valueType)) return false;
            }
            return true;
        }
    }
}