using UnityEditor;

namespace Validatox.Editor
{
    internal static class MenuItems
    {
        [MenuItem("Window/Validatox/Hub", false, 0)]
        internal static void Hub()
        {
            EditorWindow.GetWindow(typeof(ValidatoxHubEditorWindow));
        }

        [MenuItem("Window/Validatox/Log", false, 0)]
        internal static void Log()
        {
            EditorWindow.GetWindow(typeof(ValidatoxLogEditorWindow));
        }

        [MenuItem("Window/Validatox/Settings", false, 0)]
        internal static void Settings()
        {
            EditorWindow.GetWindow(typeof(ValidatoxSettingsEditorWindow));
        }
    }
}