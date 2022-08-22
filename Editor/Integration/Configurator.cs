using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

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
                IsPackage = IsPackage = request.Result.FirstOrDefault(p => p.name.Equals(Resources.PackageId)) is not null;
                EditorApplication.update -= EnvironmentSetup;
            }
        }
    }
}