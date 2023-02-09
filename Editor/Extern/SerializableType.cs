using System;

namespace Validatox.Editor.Extern
{
    /// <summary>
    ///   Make a <see cref="Type"/> serializable through its assembly qualified name
    /// </summary>
    [System.Serializable]
    public class SerializableType : IEquatable<SerializableType>
    {
        private string name;
        private string assemblyQualifiedName;
        private string assemblyName;
        private Type type = null;

        public SerializableType(System.Type type)
        {
            this.type = type;
            name = type.Name;
            assemblyQualifiedName = type.AssemblyQualifiedName;
            assemblyName = type.Assembly.FullName;
        }

        public static bool operator ==(SerializableType a, SerializableType b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) ^ ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }

        public static implicit operator SerializableType(Type type) => new SerializableType(type);

        public static implicit operator Type(SerializableType type) => type.GetType();

        public bool TryResolve()
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(assemblyName)) return false;
            try
            {
                type = Type.GetType(assemblyQualifiedName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializableType == false)
            {
                return false;
            }
            return Equals(obj as SerializableType);
        }

        public bool Equals(SerializableType obj)
        {
            return obj.GetType().Equals(type);
        }

        public override int GetHashCode()
        {
            return type.GetHashCode();
        }

        public new Type GetType()
        {
            return TryResolve() ? type : type;
        }
    }
}