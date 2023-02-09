using Pury.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Validators;
using Validatox.Editor.Validators.Fix;

namespace Validatox.Editor
{
    [CustomEditor(typeof(Validator), true)]
    public class ValidatorCustomEditor : UnityEditor.Editor
    {
        protected Validator validator;
        private Vector2 scrollPos;
        private bool resultsFoldout = true;
        private GUIStyle labelStyle;
        private GUIStyle foldoutStyle;
        private GUIStyle indexStyle;
        private GUIStyle infoStyle;
        private PurySeparator separator;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            if (serializedObject.ApplyModifiedProperties() && validator.TryGetCachedResult(out _))
            {
                serializedObject.Update();
                serializedObject.FindProperty("dirtyResult").boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void DrawValidationFailures(IEnumerable<ValidationFailure> failures)
        {
            if (failures.Any(f => f.TryGetFix(out var fix) && fix.ContextlessFix && f.TryGetSubject(out _)))
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(EditorGUIUtility.TrTextContentWithIcon(" Automatic Fix", "CustomTool@2x")))
                {
                    if (EditorUtility.DisplayDialog($"Automatic Fix", $"Are you sure you want to attempt automatic fixing? " +
                        $"Depending on the implementation of fixers this may cause unwanted changes\n", "Yes", "Cancel"))
                    {
                        var res = validator.ApplyAutomaticFixes();
                        var builder = new StringBuilder("Fixes applied\n");
                        foreach (var f in res)
                        {
                            builder.Append(f.ToString());
                            builder.Append("\n");
                        }
                        EditorUtility.DisplayDialog("Automatic Fix result", builder.ToString(), "Close");
                    }
                }
                EditorGUILayout.LabelField("Automatic fixes are applied to all fixes that have the flag ContextLessFixer enabled." +
                    " The button will look for all the failures that have a fixer that can be automatically applied", infoStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }

            foldoutStyle ??= EditorStyles.foldoutHeader.Copy(a =>
            {
                a.richText = true;
                a.fontSize = 14;
            });
            resultsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(resultsFoldout, "Failures", foldoutStyle);
            EditorGUILayout.Space(5);
            if (resultsFoldout)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                for (int i = 0; i < failures.Count(); i++)
                {
                    var failure = failures.ElementAt(i);
                    if (!failure.TryGetFix(out var fix) || !failure.TryGetSubject(out _))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent($"{i + 1} | "), indexStyle);
                        EditorGUILayout.LabelField(new GUIContent(failure.ToString()), labelStyle);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent($"{i + 1} | "), indexStyle);
                        if (GUILayout.Button("Fix", GUILayout.Width(50)))
                        {
                            ValidationFixWindow.Open(fix,
                                new ValidationFixWindowOptions()
                                {
                                    Position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition)
                                });
                        }
                        EditorGUILayout.LabelField(new GUIContent(failure.ToString()), labelStyle);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                    separator.Draw();
                }

                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual void DrawProperties()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate"))
            {
                validator.Validate((progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue));
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
                EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_close") :
                EditorGUIUtility.TrTextContentWithIcon($" Validation completed with {result.Failures.Count} errors", "winbtn_mac_max") : EditorGUIUtility.TrIconContent("winbtn_mac_min");

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

                DrawValidationFailures(result.Failures);
            }
        }

        protected virtual void OnEnable()
        {
            validator = target as Validator;
            labelStyle = new GUIStyle()
            {
                wordWrap = true,
                imagePosition = ImagePosition.ImageLeft,
                richText = true,
                margin = new RectOffset(0, 0, 5, 5),
            };
            labelStyle.normal.textColor = new Color(0.75F, 0.75F, 0.75F);

            infoStyle = new GUIStyle()
            {
                wordWrap = true,
                fontStyle = FontStyle.Italic,
                richText = true,
                fontSize = 10
            };
            infoStyle.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);
            indexStyle = labelStyle.Copy(s =>
            {
                s.fontSize = 12;
                s.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);
                s.fontStyle = FontStyle.Bold;
            });
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(new RectOffset(5, 5, 8, 8));
        }
    }
}