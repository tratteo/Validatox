using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal static class MenuItems
    {
        [MenuItem("Validatox/Credits", false, 128)]
        internal static void Credits()
        {
            EditorWindow.GetWindow(typeof(CreditsEditorWindow)).titleContent = new GUIContent("Validatox");
        }

        [MenuItem("Validatox/Hub", false, 0)]
        internal static void Hub()
        {
            EditorWindow.GetWindow(typeof(ValidatoxHubEditorWindow));
        }

        [MenuItem("Validatox/Log", false, 0)]
        internal static void Log()
        {
            EditorWindow.GetWindow(typeof(ValidatoxLogEditorWindow));
        }
    }
}