using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class Resources
    {
        public static Texture2D Icon() => AssetDatabase.LoadAssetAtPath<Texture2D>($"{GetPackageRoot()}/Editor/Icons/validatox_icon.png");

        public static string GetPackageRoot() => Configurator.IsPackage ? "Packages/com.siamango.validatox" : "Assets/Siamango/Validatox";
    }
}