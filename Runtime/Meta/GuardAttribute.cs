using System;
using UnityEngine;

namespace Validatox.Meta
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GuardAttribute : PropertyAttribute
    {
        public enum Severity
        { Info, Warning, Error }

        public string Message { get; private set; }

        public Severity SeverityType { get; private set; }

        public GuardAttribute() : this(string.Empty, Severity.Warning)
        {
        }

        public GuardAttribute(Severity gravity) : this(string.Empty, gravity)
        {
        }

        public GuardAttribute(string message) : this(message, Severity.Warning)
        {
        }

        public GuardAttribute(string message, Severity gravity)
        {
            Message = message;
            SeverityType = gravity;
        }
    }
}