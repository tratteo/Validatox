using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class Resources
    {
        public const string IconsPath = ValidatoxManager.PackageEditorPath + "/Icons";
        public static readonly Texture2D Logo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/validatox_logo_nobg.png");
        public static readonly Texture2D Icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/validatox_icon.png");
        public static readonly Texture2D Github = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/github.png");
    }
}