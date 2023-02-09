using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    public static class EditorExtensions
    {
        private static GetFieldInfoAndStaticTypeFromProperty getFieldInfoAndStaticTypeFromPropertyDelegate;

        private delegate FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty property, out Type type);

        /// <summary>
        ///   Get the <see cref="Type"/> and <see cref="FieldInfo"/> for the specified <see cref="SerializedProperty"/>
        /// </summary>
        /// <param name="prop"> </param>
        /// <param name="type"> </param>
        /// <returns> </returns>
        public static FieldInfo GetFieldInfoAndStaticType(this SerializedProperty prop, out Type type)
        {
            if (getFieldInfoAndStaticTypeFromPropertyDelegate == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.Name == "ScriptAttributeUtility")
                        {
                            var mi = t.GetMethod("GetFieldInfoAndStaticTypeFromProperty", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            getFieldInfoAndStaticTypeFromPropertyDelegate = (GetFieldInfoAndStaticTypeFromProperty)Delegate.CreateDelegate(typeof(GetFieldInfoAndStaticTypeFromProperty), mi);
                            break;
                        }
                    }
                    if (getFieldInfoAndStaticTypeFromPropertyDelegate != null) break;
                }
                if (getFieldInfoAndStaticTypeFromPropertyDelegate == null)
                {
                    Debug.LogError("GetFieldInfoAndStaticType::Reflection failed!");
                    type = null;
                    return null;
                }
            }
            return getFieldInfoAndStaticTypeFromPropertyDelegate(prop, out type);
        }

        /// <summary>
        ///   Get a custom attribute for the specified <see cref="SerializedProperty"/>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="prop"> </param>
        /// <returns> </returns>
        public static T GetCustomAttributeFromProperty<T>(this SerializedProperty prop) where T : System.Attribute
        {
            var info = prop.GetFieldInfoAndStaticType(out _);
            return info.GetCustomAttribute<T>();
        }

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

        public static string ToHex(this Color color)
        {
            var sBuilder = new StringBuilder();
            sBuilder.Append(Mathf.RoundToInt(color.a * 255).ToString("X2"));
            sBuilder.Append(Mathf.RoundToInt(color.r * 255).ToString("X2"));
            sBuilder.Append(Mathf.RoundToInt(color.g * 255).ToString("X2"));
            sBuilder.Append(Mathf.RoundToInt(color.b * 255).ToString("X2"));
            return sBuilder.ToString();
        }

        public static Color HexToColor(this string hex)
        {
            if (hex.Length < 6) return Color.black;
            var a = 1F;
            var index = 0;
            if (hex.Length == 8)
            {
                a = Convert.ToInt32(hex.Substring(index, 2), 16) / 255F;
                index += 2;
            }
            var r = Convert.ToInt32(hex.Substring(index, 2), 16) / 255F;
            index += 2;
            var g = Convert.ToInt32(hex.Substring(index, 2), 16) / 255F;
            index += 2;
            var b = Convert.ToInt32(hex.Substring(index, 2), 16) / 255F;
            index += 2;
            return new Color(r, g, b, a);
        }
    }
}