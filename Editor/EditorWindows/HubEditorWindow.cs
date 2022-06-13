using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    internal class HubEditorWindow : EditorWindow
    {
        private enum Filter
        { NotValidated, Validated, All }

        private enum Context
        { Groups, Validators, Guard }

        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle infoStyle;
        private Context context;
        private Vector2 scrollPos;
        private Filter appliedFilter;

        private void OnEnable()
        {
            autoRepaintOnSceneChange = true;
            minSize = new Vector2(512, 512);
            context = Context.Groups;

            infoStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Italic,
                fontSize = 10
            };
            infoStyle.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);

            textStyle = new GUIStyle()
            {
                imagePosition = ImagePosition.ImageLeft,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 5, 5),
                fontSize = 16
            };
            textStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 10, 20)
            };
            titleStyle.normal.textColor = Color.white;

            appliedFilter = Filter.All;
            titleContent.image = Resources.LogoPadded;
            titleContent.tooltip = "Get validated :D";
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("window", GUILayout.MaxWidth(200), GUILayout.ExpandHeight(true));
            var style = GUI.skin.button;
            style.margin = new RectOffset(10, 10, 10, 10);
            var values = Enum.GetValues(typeof(Context));
            foreach (var value in values)
            {
                if (GUILayout.Button(value.ToString(), style))
                {
                    ChangeContext((Context)value);
                }
            }
            GUILayout.EndVertical();
            style = GUI.skin.label;
            style.margin = new RectOffset(10, 10, 10, 10);
            GUILayout.BeginVertical(style, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawContext();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawValidatorAsset(Validator validatorBase)
        {
            var verticalStyle = GUI.skin.box;
            verticalStyle.margin = new RectOffset(10, 10, 10, 10);
            GUILayout.BeginVertical(verticalStyle, GUILayout.ExpandWidth(true));
            var content = EditorGUIUtility.ObjectContent(validatorBase, validatorBase.GetType());
            content.text = " " + validatorBase.name;
            GUILayout.Label(content, textStyle, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 2));
            var hasResult = validatorBase.TryGetCachedResult(out var result);
            GUILayout.Space(5);
            if (validatorBase.DirtyResult)
            {
                GUILayout.Label(EditorGUIUtility.TrTextContentWithIcon(" Asset may have changed since last validation", "d_console.warnicon.sml"), infoStyle, GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("State:", GUILayout.Width(40));
            content = hasResult ? result.Successful ? EditorGUIUtility.TrIconContent("winbtn_mac_max") : EditorGUIUtility.TrIconContent("winbtn_mac_close") : EditorGUIUtility.TrIconContent("winbtn_mac_min");
            GUILayout.Label(content, GUILayout.MaxWidth(20));
            if (hasResult && !result.Successful)
            {
                GUILayout.Label($"{result.Failures.Count} errors");
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Time:", GUILayout.Width(40));
            content = hasResult ? EditorGUIUtility.TrTextContentWithIcon(result.Time.ToString(), "TestStopwatch") : GUIContent.none;
            GUILayout.Label(content, GUILayout.MaxWidth(250));
            GUILayout.EndHorizontal();

            if (validatorBase is GroupValidator group)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Label("Size:", GUILayout.Width(40));
                GUILayout.Label(group.GetValidators().Count.ToString(), GUILayout.MaxWidth(250));
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Validate"), GUILayout.MaxWidth(100)))
            {
                validatorBase.Validate(EditorProgressReport);
                EditorUtility.ClearProgressBar();
            }
            EditorGUI.BeginDisabledGroup(!hasResult);
            if (GUILayout.Button(new GUIContent("Clear"), GUILayout.MaxWidth(100)))
            {
                validatorBase.ClearResult();
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_ViewToolZoom"), GUILayout.Width(25), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                Selection.activeObject = validatorBase;
            }
            if (GUILayout.Button(EditorGUIUtility.TrIconContent("TreeEditor.Trash"), GUILayout.Width(25), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                if (EditorUtility.DisplayDialog($"Delete {validatorBase.name}", $"Are you sure you want to delete the group {validatorBase.name}?\n", "Yes", "Cancel"))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(validatorBase));
                }
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private List<Validator> ApplyFilter(List<Validator> validators, Filter filter)
        {
            Predicate<Validator> filterPredicate = null;
            filterPredicate = filter switch
            {
                Filter.NotValidated => v => !v.TryGetCachedResult(out _),
                Filter.Validated => v => v.TryGetCachedResult(out _),
                Filter.All => v => true,
                _ => v => true,
            };
            return validators.FindAll(filterPredicate);
        }

        private List<Validator> GetValidatorsByContext()
        {
            var validators = ValidatoxTools.GetAllBehavioursInAsset<Validator>();
            validators = ApplyFilter(validators, appliedFilter);
            switch (context)
            {
                case Context.Groups:
                    validators = validators.FindAll(v => v is GroupValidator);
                    break;

                case Context.Validators:
                    validators = validators.FindAll(v => v is not GroupValidator or GuardValidator);
                    break;

                case Context.Guard:
                    break;

                default:
                    break;
            }
            return validators;
        }

        private void DrawContext()
        {
            GUILayout.Label(context.ToString(), titleStyle, GUILayout.ExpandWidth(true));
            if (context is Context.Guard)
            {
                return;
            }
            var validators = GetValidatorsByContext();
            var labelStyle = GUI.skin.label;
            labelStyle.alignment = TextAnchor.LowerLeft;
            labelStyle.margin = new RectOffset();
            GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            EditorGUI.BeginDisabledGroup(validators.Count <= 0);
            if (GUILayout.Button("Delete all", GUILayout.MaxWidth(100)))
            {
                if (EditorUtility.DisplayDialog($"Delete all", $"Are you sure you want to delete all {context}?\n", "Yes", "Cancel"))
                {
                    var failed = new List<string>();
                    AssetDatabase.DeleteAssets((from a in validators select AssetDatabase.GetAssetPath(a)).ToArray(), failed);
                    ShowNotification(new GUIContent($"Successfully deleted {validators.Count - failed.Count}/{validators.Count}"), 0.5F);
                }
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Label(GUIContent.none, GUILayout.ExpandWidth(true));
            appliedFilter = (Filter)EditorGUILayout.EnumPopup(appliedFilter, GUILayout.MaxWidth(150));
            GUILayout.EndHorizontal();
            GUILayout.Label("Count: " + validators.Count, infoStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.window, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            var rectStyle = GUIStyle.none;
            rectStyle.margin = new RectOffset(10, 10, 0, 0);
            foreach (var group in validators)
            {
                DrawValidatorAsset(group);
                GUILayout.Space(5);
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1, rectStyle), new Color(0.5F, 0.5F, 0.5F, 1));
                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();
        }

        private void EditorProgressReport(ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue);

        private void ChangeContext(Context context)
        {
            this.context = context;
        }
    }
}