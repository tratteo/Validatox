using System;
using UnityEngine;

namespace Validatox.Meta
{
    /// <summary>
    ///   Exclude certain subtypes to be assigned in the Editor inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ExcludeSubtypesAttribute : PropertyAttribute
    {
        public Type[] ExcludedTypes { get; private set; }

        public bool ExcludeSubclasses { get; set; }

        public ExcludeSubtypesAttribute(params Type[] subtypes)
        {
            ExcludedTypes = subtypes;
        }
    }
}