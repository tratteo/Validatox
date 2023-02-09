using Pury.Editor;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   A fix for a <see cref="ValidationFailure"/>
    /// </summary>
    public abstract class ValidationFix
    {
        public enum OpenStyle
        { Popup, Utility, Normal }

        private string homeScene;
        private string activeSceneCache;
        private GUIStyle titleStyle;
        private PurySeparator separator;

        /// <summary>
        ///   Determine the window style for the fix
        /// </summary>
        public OpenStyle Style { get; protected set; } = OpenStyle.Popup;

        /// <summary>
        ///   Whether the current context (Scene) is the scene of the subject. Always true if the object is an asset
        /// </summary>
        public bool IsInContext { get; private set; }

        /// <summary>
        ///   The title of the window
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        ///   Whether this fix can be applied without context. Fixes that has this flag set to true are eligible for automatic fixing by the
        ///   <see cref="Validator"/>
        /// </summary>
        public bool ContextlessFix { get; protected set; } = false;

        /// <summary>
        ///   Size of the window
        /// </summary>
        public Vector2 Size { get; protected set; } = new Vector2(300, 100);

        /// <summary>
        ///   Whether the subject of the <see cref="ValidationFailure"/> belongs to a scene
        /// </summary>
        protected bool IsSceneObject { get; private set; }

        /// <summary>
        ///   The failure this fix corresponds to
        /// </summary>
        protected ValidationFailure Failure { get; private set; }

        /// <summary>
        ///   Arguments passed to the fix. Note that only serializable arguments are passed, since fixes are serialized by the editor
        /// </summary>
        protected object[] Args { get; private set; }

        protected ValidationFix(ValidationFailure failure, params object[] args)
        {
            Failure = failure;
            Args = args;
            failure.TryGetSubject(out var subject);
            IsSceneObject = failure.ScenePath != null;
            IsInContext = !IsSceneObject || SceneManager.GetActiveScene().path == failure.ScenePath;
            Title = $"Fix for {(subject ? subject.name : failure.ScenePath)}";
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(new RectOffset(5, 5, 5, 5));
        }

        /// <summary>
        ///   Create a new <see cref="ValidationFix"/>
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="failure"> </param>
        /// <param name="args"> </param>
        /// <returns> </returns>
        public static ValidationFix InstantiateFix(Type type, ValidationFailure failure, params object[] args) =>
            (ValidationFix)Activator.CreateInstance(type, new object[] { failure, args });

        public override string ToString() => GetLabel();

        /// <summary>
        ///   Render the fix content
        /// </summary>
        /// <param name="window"> </param>
        public void Render(ValidationFixWindow window)
        {
            EditorRender(window);
            GUILayout.FlexibleSpace();
            var activeScene = SceneManager.GetActiveScene();
            if (IsSceneObject && activeScene.path != Failure.ScenePath)
            {
                IsInContext = false;
                EditorGUILayout.LabelField("The object belongs to scene " + Failure.ScenePath);
                if (GUILayout.Button("Open scene"))
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    homeScene = activeScene.path;
                    EditorSceneManager.OpenScene(Failure.ScenePath);
                }
            }
            else
            {
                IsInContext = true;
            }
            if (homeScene != null)
            {
                if (GUILayout.Button("Return to " + homeScene))
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(homeScene);
                    homeScene = null;
                }
            }
        }

        /// <summary>
        ///   Apply the fix
        /// </summary>
        public void ApplyFix()
        {
            activeSceneCache = SceneManager.GetActiveScene().path;
            if (IsSceneObject && activeSceneCache != Failure.ScenePath)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(Failure.ScenePath);
            }
            if (Failure.TryGetSubject(out var obj))
            {
                var serializedObject = new SerializedObject(obj);
                if (serializedObject != null)
                {
                    var success = Fix(serializedObject);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    if (success)
                    {
                        Failure.Validator.PerformSingleValidation(obj, Failure);
                    }
                }
                else
                {
                    Debug.LogWarning("Unable to locate serialize object, check that the fix was provided with a subject");
                }
            }
            else
            {
                Debug.LogWarning("Missing subject, check that the fix was provided with a subject");
            }
            if (IsSceneObject && activeSceneCache != Failure.ScenePath)
            {
                EditorSceneManager.SaveScene(SceneManager.GetSceneByPath(Failure.ScenePath));
                EditorSceneManager.OpenScene(activeSceneCache);
            }
        }

        public virtual string GetLabel() => GetType().Name;

        protected abstract bool Fix(SerializedObject serializedObject);

        protected virtual void EditorRender(ValidationFixWindow window)
        {
            titleStyle ??= GUI.skin.label.Copy(s =>
            {
                s.fontStyle = FontStyle.Bold;
                s.wordWrap = true;
                s.richText = true;
            });
            var content = EditorGUIUtility.TrTextContentWithIcon($" {GetLabel()}", "d_CustomTool");

            EditorGUILayout.LabelField(content, titleStyle);
            //separator.Draw();
        }

        /// <summary>
        ///   Display a generic error label
        /// </summary>
        /// <param name="error"> </param>
        protected void ErrorLabel(string error)
        {
            EditorGUILayout.LabelField($"The fix cannot be applied: {error}",
                EditorStyles.wordWrappedLabel.Copy(s => s.normal.textColor = new Color(0.75F, 0.15F, 0.12F)));
        }

        /// <summary>
        ///   Perform a complete validation of the <see cref="ValidationFailure.Validator"/>
        /// </summary>
        protected void ExecuteValidation()
        {
            if (Failure.Validator.DirtyResult)
            {
                Failure.Validator.Validate();
            }
        }
    }
}