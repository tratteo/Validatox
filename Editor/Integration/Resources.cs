using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class Resources
    {
        public const string IconsPath = ValidatoxManager.PackageEditorPath + "/Icons";
        public static readonly Texture2D Logo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/logo.png");
        public static readonly Texture2D LogoPadded = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/logoPadded.png");
        public static readonly Texture2D Title = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/title.png");
        public static readonly Texture2D Github = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconsPath}/github.png");
    }
}