using System.IO;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class Resources
    {
        public const string PackageId = "com.tratteo.validatox";
        public const string Author = "tratteo";
        public const string Name = "Validatox";
        private static string projectRoot;

        public static Texture2D Icon()
        {
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath($"{GetPackageRoot()}/Editor/Icons/validatox_icon.png", typeof(Texture2D));
            return texture;
        }

        public static string GetPackageRoot()
        {
            if (projectRoot != null) return projectRoot;
            var res = UnityEngine.Resources.Load("82e292f8-9796-4fb6-a9cb-45bb54e0421e");
            if (!res)
            {
                Debug.LogWarning("Unable to find Validatox location marker prefab in root folder");
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
                return packageInfo != null ? $"Packages/{PackageId}" : $"Assets/{Author}/{Name}";
            }
            projectRoot = Path.GetDirectoryName(AssetDatabase.GetAssetPath(res)).Replace("Resources", "");
            return projectRoot;
        }
    }
}