using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class Resources
    {
        public const string PackageId = "com.tratteo.validatox";
        public const string Author = "tratteo";
        public const string Name = "Validatox";

        public static Texture2D Icon() => AssetDatabase.LoadAssetAtPath<Texture2D>($"{GetPackageRoot()}/Editor/Icons/validatox_icon.png");

        public static string GetPackageRoot() => Configurator.IsPackage ? $"Packages/{PackageId}" : $"Assets/{Author}/{Name}";
    }
}