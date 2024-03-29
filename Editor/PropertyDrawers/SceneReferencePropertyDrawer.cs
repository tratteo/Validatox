﻿using UnityEditor;
using UnityEngine;
using Validatox.Serializable;

namespace Validatox.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var scenePathProp = property.FindPropertyRelative("path");
            var sceneNameProp = property.FindPropertyRelative("name");

            var scene = EditorGUI.ObjectField(position, label, GetSceneAsset(sceneNameProp.stringValue).Item1, typeof(SceneAsset), true);
            if (!scene)
            {
                property.serializedObject.Update();
                scenePathProp.stringValue = string.Empty;
                sceneNameProp.stringValue = string.Empty;
                property.serializedObject.ApplyModifiedProperties();
            }
            else if (scene.name != sceneNameProp.stringValue)
            {
                var (reference, path) = GetSceneAsset(scene.name);
                if (reference)
                {
                    property.serializedObject.Update();
                    sceneNameProp.stringValue = scene.name;
                    scenePathProp.stringValue = path;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndProperty();
        }

        protected (SceneAsset, string) GetSceneAsset(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName)) return default;

            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.path.IndexOf(sceneAssetName) != -1)
                {
                    return (AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset, editorScene.path);
                }
            }
            Debug.LogWarning("Unable to find scene [" + sceneAssetName + "] in build settings");
            return default;
        }
    }
}