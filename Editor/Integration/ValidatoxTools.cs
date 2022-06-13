using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Validatox.Editor
{
    public static class ValidatoxTools
    {
        /// <summary>
        /// </summary>
        /// <param name="root"> </param>
        /// <param name="exclusion"> Folders name to exclude from the query </param>
        /// <returns> All <see cref="UnityEngine.Object"/> in the specified path </returns>
        public static List<UnityEngine.Object> GetUnityObjectsAtPath(string path, params string[] exclusion)
        {
            if (!Directory.Exists(path)) return new List<UnityEngine.Object>();
            var inAsset = path.Contains(Application.dataPath);
            if (inAsset)
            {
                path = path.Replace(Application.dataPath, string.Empty);
                path = string.IsNullOrWhiteSpace(path) ? "Assets" : $"Assets{path}";
            }
            var exclusionList = exclusion.ToList();
            var sel = new List<UnityEngine.Object>();
            var fileEntries = Directory.GetFiles(path);
            foreach (var file in fileEntries)
            {
                var fileName = Path.GetFileName(file);
                var asset = AssetDatabase.LoadMainAssetAtPath($"{path}{Path.DirectorySeparatorChar}{fileName}");
                if (!asset) continue;
                sel.Add(asset);
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var dirInfo = new DirectoryInfo(dir);
                if (exclusionList.Contains(dirInfo.Name)) continue;
                var localPath = $"{path}{Path.DirectorySeparatorChar}{dirInfo.Name}";
                sel.AddRange(GetUnityObjectsAtPath(localPath));
            }
            return sel;
        }

        /// <summary>
        ///   <inheritdoc cref="GetAllBehaviours{T}(Type[], UnityEngine.Object[])"/>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="objs"> </param>
        /// <returns> </returns>
        public static List<T> GetAllBehaviours<T>(params UnityEngine.Object[] objs) where T : class => GetAllBehaviours<T>(new Type[0], objs);

        /// <summary>
        ///   For each object:
        ///   <list>
        ///     <item>
        ///       <description> - If it is a <see cref="GameObject"/>, retrieve all components of type T </description>
        ///     </item>
        ///     <item>
        ///       <description> - If it is a <see cref="ScriptableObject"/>, retrieve it if it's of type T </description>
        ///     </item>
        ///   </list>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="objs"> </param>
        /// <returns> </returns>
        public static List<T> GetAllBehaviours<T>(Type[] exclusions, params UnityEngine.Object[] objs) where T : class
        {
            var behaviours = new List<T>();
            if (exclusions.Contains(typeof(T))) return behaviours;
            foreach (var obj in objs)
            {
                if (typeof(T).IsSubclassOf(typeof(ScriptableObject)))
                {
                    if (exclusions.Contains(obj.GetType())) continue;
                    if (typeof(T).Equals(typeof(UnityEngine.Object)))
                    {
                        behaviours.Add(obj as T);
                    }
                    else if (obj is T so)
                    {
                        behaviours.Add(so);
                    }
                }
                else if (typeof(T).IsSubclassOf(typeof(Component)) && obj is GameObject gObj)
                {
                    behaviours.AddRange(gObj.GetComponents<T>());
                }
            }
            return behaviours;
        }

        /// <summary>
        /// </summary>
        /// <param name="root"> The root path to start searching from </param>
        /// <param name="exclusion"> Folders name to exclude from the query </param>
        /// <returns> All <see cref="UnityEngine.Object"/> in the Asset folder </returns>
        public static List<UnityEngine.Object> GetUnityObjectsInAssets(string root = "", params string[] exclusion)
        {
            var compositePath = string.IsNullOrWhiteSpace(root) ? Application.dataPath : $"{Application.dataPath}{Path.AltDirectorySeparatorChar}{root}";
            return GetUnityObjectsAtPath(compositePath, exclusion);
        }

        /// <summary>
        ///   Applied to all <see cref="UnityEngine.Object"/> in the specified <i> path </i> folder. <inheritdoc cref="Gib.GetAllBehaviours{T}(UnityEngine.Object[])"/>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="root"> </param>
        /// <returns> </returns>
        public static List<T> GetAllBehavioursAtPath<T>(string path, params Type[] exclusions) where T : UnityEngine.Object => GetAllBehaviours<T>(exclusions, GetUnityObjectsAtPath(path).ToArray());

        /// <summary>
        ///   Applied to all <see cref="UnityEngine."/> in the <i> Asset </i> folder. <inheritdoc cref="Gib.GetAllBehaviours{T}(UnityEngine.Object[])"/>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="root"> </param>
        /// <returns> </returns>
        public static List<T> GetAllBehavioursInAsset<T>(string root = "", params Type[] exclusions) where T : UnityEngine.Object => GetAllBehaviours<T>(exclusions, GetUnityObjectsInAssets(root).ToArray());

        /// <summary>
        ///   Execute an <see cref="Action"/> on all the <see cref="GameObject"/> in the specified scene
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="scenePath"> </param>
        /// <param name="action"> </param>
        /// <returns> The number of <see cref="GameObject"/> the <see cref="Action"/> has been run on </returns>
        public static int ExecuteForGameObjectsInScene(string scenePath, Action<GameObject> action = null)
        {
            var count = 0;
            var openedScene = SceneManager.GetActiveScene();
            var openedScenePath = openedScene.path;
            var isOpenedScene = openedScenePath.Equals(scenePath);
            if (!isOpenedScene)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }
            var sceneRef = isOpenedScene ? openedScene : EditorSceneManager.OpenScene(scenePath);

            var scenesObjs = sceneRef.GetRootGameObjects();

            foreach (var obj in scenesObjs)
            {
                var children = obj.GetComponentsInChildren<Transform>(true);
                foreach (var t in children)
                {
                    count++;
                    action?.Invoke(t.gameObject);
                }
            }
            if (!isOpenedScene) EditorSceneManager.OpenScene(openedScenePath);
            return count;
        }

        /// <summary>
        ///   Execute an <see cref="Action"/> on all the components of type T in the specified scene
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="scenePath"> </param>
        /// <param name="action"> </param>
        /// <returns> The number of components the <see cref="Action"/> has been run on </returns>
        public static int ExecuteForComponentsInScene<T>(string scenePath, Action<T> action = null) where T : Component
        {
            var count = 0;
            ExecuteForGameObjectsInScene(scenePath, obj =>
            {
                var comps = obj.GetComponents<T>();
                count += comps.Length;
                foreach (var c in comps) action?.Invoke(c);
            });

            return count;
        }
    }
}