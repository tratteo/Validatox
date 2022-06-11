﻿using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal static class MenuItems
    {
        [MenuItem("Validatox/Validate groups", false, 4)]
        internal static void ValidateAllGroups() => Validatox.ValidateAllGroups();

        [MenuItem("Validatox/Validate guardeds", false, 8)]
        internal static void ValidateGuardeds() => Validatox.ValidateGuarded();

        [MenuItem("Validatox/Validate", false, 0)]
        internal static void Validate()
        {
            Validatox.ValidateAllGroups();
            Validatox.ValidateGuarded();
        }

        [MenuItem("Validatox/Credits", false, 128)]
        internal static void Credits()
        {
            EditorWindow.GetWindow(typeof(CreditsEditorWindow)).titleContent = new GUIContent("Validatox");
        }
    }
}