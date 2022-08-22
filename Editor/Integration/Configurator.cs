using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Validatox.Editor
{
    [InitializeOnLoad]
    internal class Configurator
    {
        private static readonly ListRequest request;

        public static bool IsPackage { get; private set; }

        static Configurator()
        {
            request = Client.List();
            EditorApplication.update += EnvironmentSetup;
        }

        private static void EnvironmentSetup()
        {
            if (request.IsCompleted)
            {
                IsPackage = request.Status == StatusCode.Success;
                Debug.Log("Environment: " + (IsPackage ? "Package" : "Asset"));
                EditorApplication.update -= EnvironmentSetup;
            }
        }
    }
}