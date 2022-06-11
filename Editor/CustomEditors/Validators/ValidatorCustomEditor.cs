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
            }

            var hasResult = validator.TryGetCachedResult(out var result);

            EditorGUI.BeginDisabledGroup(!hasResult);
            if (GUILayout.Button("Clear"))
            {
                validator.ClearResult();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if (hasResult)
            {
                EditorGUILayout.Space(10);
                var rect = EditorGUILayout.GetControlRect(false, 2);
                EditorGUI.DrawRect(rect, new Color(0.5F, 0.5F, 0.5F, 1));
                EditorGUILayout.Space(10);
                var content = !result.Successful ?
                    EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_close@2x") :
                    EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_max@2x");

                EditorGUILayout.LabelField(content, labelStyle);
                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon($" {result.Time}", "TestStopwatch"), labelStyle);

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
                imagePosition = ImagePosition.ImageLeft
            };
            labelStyle.normal.textColor = Color.white;
        }

        private void EditorProgressReport(ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue);
    }
}