using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal static class MenuItems
    {
        [MenuItem("Validatox/Validate groups", false, 4)]
        internal static void ValidateAllGroups() => ValidatoxManager.ValidateAllGroups();

        [MenuItem("Validatox/Validate guardeds", false, 8)]
        internal static void ValidateGuardeds() => ValidatoxManager.ValidateGuarded();

        [MenuItem("Validatox/Validate", false, 4)]
        internal static void Validate()
        {
            ValidatoxManager.ValidateAllGroups();
            ValidatoxManager.ValidateGuarded();
        }

        [MenuItem("Validatox/Credits", false, 128)]
        internal static void Credits()
        {
            EditorWindow.GetWindow(typeof(CreditsEditorWindow)).titleContent = new GUIContent("Validatox");
        }

        [MenuItem("Validatox/Hub", false, 0)]
        internal static void Hub()
        {
            EditorWindow.GetWindow(typeof(HubEditorWindow)).titleContent = new GUIContent("Validatox Hub");
        }
    }
}