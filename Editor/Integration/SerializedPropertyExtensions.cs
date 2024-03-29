﻿using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        ///   Get the value of a serialized property
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="property"> </param>
        /// <returns> </returns>
        public static T GetValue<T>(this SerializedProperty property) where T : class
        {
            object obj = property.serializedObject.targetObject;
            var path = property.propertyPath.Replace(".Array.data", "");
            var fieldStructure = path.Split('.');
            var rgx = new Regex(@"\[\d+\]");
            for (var i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    var index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }
            return (T)obj;
        }

        /// <summary>
        ///   Set the value of a serialized property
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="property"> </param>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public static bool SetValue<T>(this SerializedProperty property, T value) where T : class
        {
            object obj = property.serializedObject.targetObject;
            var path = property.propertyPath.Replace(".Array.data", "");
            var fieldStructure = path.Split('.');
            var rgx = new Regex(@"\[\d+\]");
            for (var i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    var index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            var fieldName = fieldStructure.Last();
            if (fieldName.Contains("["))
            {
                var index = System.Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
                return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
            }
            else
            {
                Debug.Log(value);
                return SetFieldValue(fieldName, obj, value);
            }
        }

        private static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            return false;
        }

        private static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                var list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    ((object[])list)[index] = value;
                    return true;
                }
                else if (value is IEnumerable)
                {
                    ((IList)list)[index] = value;
                    return true;
                }
            }
            return false;
        }

        private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            return default(object);
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                var list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    return ((object[])list)[index];
                }
                else if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }
            return default(object);
        }
    }
}