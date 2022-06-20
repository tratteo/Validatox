using UnityEditor;
using UnityEngine;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    [CustomEditor(typeof(Validator), true)]
    public class ValidatorCustomEditor : UnityEditor.Editor
    {
        protected Validator validator;
        private Vector2 scrollPos;
        private bool resultsFoldout;
        private GUIStyle labelStyle;
        private GUIStyle infoStyle;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawProperties()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate"))
            {
                validator.Validate(EditorProgressReport);
                EditorUtility.ClearProgressBar();
                validator.LogResult();
            }

            var hasResult = validator.TryGetCachedResult(out var result);

            EditorGUI.BeginDisabledGroup(!hasResult);
            if (GUILayout.Button("Clear"))
            {
                validator.ClearResult();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), new Color(0.5F, 0.5F, 0.5F, 1));
            EditorGUILayout.Space(10);
            var content = hasResult ? !result.Successful ?
                EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_close@2x") :
                EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_max@2x") : EditorGUIUtility.TrIconContent("winbtn_mac_min");

            EditorGUILayout.LabelField(content, labelStyle);
            if (hasResult)
            {
                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon($" {result.Time}", "TestStopwatch"), labelStyle);

                if (validator.DirtyResult)
                {
                    GUILayout.Label(EditorGUIUtility.TrTextContentWithIcon(" Asset may have changed since last validation", "d_console.warnicon.sml"), infoStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);
                }

                EditorGUILayout.Space(10);
                resultsFoldout = EditorGUILayout.Foldout(resultsFoldout, "Failures");
                EditorGUILayout.Space(5);
                if (resultsFoldout)
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    foreach (var failure in result.Failures)
                    {
                        EditorGUILayout.LabelField(new GUIContent(failure.ToString()), labelStyle);
                        EditorGUILayout.Space(5);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        protected virtual void OnEnable()
        {
            validator = target as Validator;
            labelStyle = new GUIStyle()
            {
                wordWrap = true,
                imagePosition = ImagePosition.ImageLeft,
                margin = new RectOffset(0, 0, 5, 5),
            };
            labelStyle.normal.textColor = Color.white;

            infoStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Italic,
                fontSize = 10
            };
            infoStyle.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);
        }

        private void EditorProgressReport(ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue);
    }
}