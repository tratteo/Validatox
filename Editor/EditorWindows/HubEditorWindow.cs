using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    internal class HubEditorWindow : EditorWindow
    {
        private enum ValidationFilter
        { NotValidated, Validated, All }

        private enum DirtyFilter
        { Dirty, Clean, All }

        private enum ResultFilter
        { Success, Failure, All }

        private enum Context
        { Groups, Validators, Guard }

        private GUIStyle textStyle;
        private GUIStyle infoStyle;
        private GUIStyle rectStyle;
        private Context context;
        private Vector2 scrollPos;
        private ValidationFilter validationFilter;
        private DirtyFilter dirtyFilter;
        private ResultFilter resultFilter;
        private GuardValidator guardValidator;

        private List<Validator> paresedValidators;

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshParsedValidators;
        }

        private void OnEnable()
        {
            RefreshParsedValidators();
            EditorApplication.projectChanged += RefreshParsedValidators;
            autoRepaintOnSceneChange = true;
            minSize = new Vector2(512, 512);
            context = Context.Groups;

            infoStyle = GUIStyle.none.Copy(s =>
            {
                s.fontStyle = FontStyle.Italic;
                s.fontSize = 10;
                s.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);
            });

            textStyle = GUIStyle.none.Copy(s =>
            {
                s.imagePosition = ImagePosition.ImageLeft;
                s.alignment = TextAnchor.MiddleLeft;
                s.padding = new RectOffset(0, 0, 5, 5);
                s.fontSize = 16;
                s.normal.textColor = Color.white;
            });

            rectStyle = GUIStyle.none.Copy(s => s.margin = new RectOffset(10, 10, 0, 0));
            validationFilter = ValidationFilter.All;
            dirtyFilter = DirtyFilter.All;
            resultFilter = ResultFilter.All;

            var guid = ValidatoxSettings.Load().GuardValidatorGuid;
            guardValidator = string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<GuardValidator>(AssetDatabase.GUIDToAssetPath(guid));

            titleContent.image = Resources.LogoPadded;
            titleContent.tooltip = "Get validated :D";
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            DrawSidebar();
            DrawContext();
            GUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            GUILayout.BeginVertical("window", GUILayout.MaxWidth(250), GUILayout.ExpandHeight(true));
            var values = Enum.GetValues(typeof(Context));
            foreach (var value in values)
            {
                if (GUILayout.Button(value.ToString(), GUI.skin.button.Copy(s => s.margin = new RectOffset(10, 10, 10, 10))))
                {
                    context = (Context)value;
                }
            }
            GUILayout.Space(25);

            GUILayout.Label("Filters", textStyle.Alignment(TextAnchor.MiddleCenter), GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Status", textStyle.FontSize(12));
            validationFilter = (ValidationFilter)EditorGUILayout.EnumPopup(validationFilter, GUILayout.ExpandWidth(true));
            if (validationFilter is ValidationFilter.All or ValidationFilter.Validated)
            {
                GUILayout.Space(5);
                GUILayout.Label("Result", textStyle.FontSize(12));
                resultFilter = (ResultFilter)EditorGUILayout.EnumPopup(resultFilter, GUILayout.ExpandWidth(true));
                GUILayout.Space(5);
                GUILayout.Label("Dirty status", textStyle.FontSize(12));
                dirtyFilter = (DirtyFilter)EditorGUILayout.EnumPopup(dirtyFilter, GUILayout.ExpandWidth(true));
            }
            GUILayout.Space(10);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1, rectStyle), new Color(0.5F, 0.5F, 0.5F, 1));
            GUILayout.Space(10);
            if (GUILayout.Button("Clear"))
            {
                validationFilter = ValidationFilter.All;
                resultFilter = ResultFilter.All;
                dirtyFilter = DirtyFilter.All;
            }
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void ChangeGuardValidator(GuardValidator overrideGuard)
        {
            if (overrideGuard)
            {
                ValidatoxSettings.Edit(s => s.GuardValidatorGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(overrideGuard)).ToString());
            }
            else
            {
                ValidatoxSettings.Edit(s => s.GuardValidatorGuid = string.Empty);
            }
            guardValidator = overrideGuard;
        }

        private void RefreshParsedValidators()
        {
            paresedValidators = ValidatoxTools.GetAllBehavioursInAsset<Validator>();
        }

        private void DrawContext()
        {
            GUILayout.BeginVertical(GUI.skin.window.Copy(s => s.margin = new RectOffset(10, 10, 10, 10)), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label(context.ToString(), GUIStyle.none.Copy(s =>
            {
                s.fontSize = 20;
                s.alignment = TextAnchor.MiddleCenter;
                s.padding = new RectOffset(0, 0, 10, 20);
                s.normal.textColor = Color.white;
            }), GUILayout.ExpandWidth(true));
            var validators = GetValidatorsByContext();
            switch (context)
            {
                case Context.Guard:
                    guardValidator = EditorGUILayout.ObjectField("Override", guardValidator, typeof(GuardValidator), false) as GuardValidator;
                    if (!guardValidator)
                    {
                        GUILayout.Label($"Using Validatox default {nameof(GuardValidator)} located in {ValidatoxManager.PackageEditorPath}", infoStyle);
                        GUILayout.Space(5);
                    }
                    ChangeGuardValidator(guardValidator);
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
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
                    if (GUILayout.Button("Validate", GUILayout.MaxWidth(100)))
                    {
                        ValidatoxManager.Validate();
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);
                    scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    foreach (var group in validators)
                    {
                        DrawValidatorAsset(group);
                        GUILayout.Space(5);
                        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1, rectStyle), new Color(0.5F, 0.5F, 0.5F, 1));
                        GUILayout.Space(5);
                    }
                    GUILayout.EndScrollView();
                    break;

                default:

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

                    GUILayout.EndHorizontal();
                    GUILayout.Label("Count: " + validators.Count, infoStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(5);
                    scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    foreach (var group in validators)
                    {
                        DrawValidatorAsset(group);
                        GUILayout.Space(5);
                        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1, rectStyle), new Color(0.5F, 0.5F, 0.5F, 1));
                        GUILayout.Space(5);
                    }
                    GUILayout.EndScrollView();

                    break;
            }
            GUILayout.EndVertical();
        }

        private void DrawValidatorAsset(Validator validatorBase)
        {
            GUILayout.BeginVertical(GUIStyle.none.Copy(s => s.margin = new RectOffset(10, 10, 10, 10)), GUILayout.ExpandWidth(true));
            var content = EditorGUIUtility.ObjectContent(validatorBase, validatorBase.GetType());
            content.text = " " + validatorBase.name;
            GUILayout.Label(content, textStyle.Copy(s => s.alignment = TextAnchor.MiddleLeft), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 2));
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
            if (validatorBase is GuardValidator guard)
            {
                EditorGUI.BeginDisabledGroup(guard == guardValidator);
                if (GUILayout.Button(new GUIContent("Assign"), GUILayout.MaxWidth(100)))
                {
                    ChangeGuardValidator(guard);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Validate"), GUILayout.MaxWidth(100)))
                {
                    validatorBase.Validate(EditorProgressReport);
                    EditorUtility.ClearProgressBar();
                }
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

        private void EditorProgressReport(ValidationProgress progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue);

        #region Filtering

        private List<Validator> ApplyFilters(List<Validator> validators)
        {
            Predicate<Validator> validationPredicate = null;
            Predicate<Validator> dirtyPredicate = null;
            Predicate<Validator> resultPredicate = null;

            validationPredicate = validationFilter switch
            {
                ValidationFilter.NotValidated => v => !v.TryGetCachedResult(out _),
                ValidationFilter.Validated => v => v.TryGetCachedResult(out _),
                ValidationFilter.All => v => true,
                _ => v => true,
            };
            if (validationFilter is ValidationFilter.NotValidated)
            {
                dirtyPredicate = s => true;
                resultPredicate = s => true;
            }
            else
            {
                dirtyPredicate = dirtyFilter switch
                {
                    DirtyFilter.Dirty => v => v.DirtyResult,
                    DirtyFilter.Clean => v => !v.DirtyResult,
                    DirtyFilter.All => v => true,
                    _ => v => true,
                };
                resultPredicate = resultFilter switch
                {
                    ResultFilter.Success => v => v.TryGetCachedResult(out var res) && res.Successful,
                    ResultFilter.Failure => v => v.TryGetCachedResult(out var res) && !res.Successful,
                    ResultFilter.All => v => true,
                    _ => v => true,
                };
            }

            return validators.FindAll(v => dirtyPredicate(v) && resultPredicate(v) && validationPredicate(v));
        }

        private List<Validator> GetValidatorsByContext()
        {
            var validators = paresedValidators;
            switch (context)
            {
                case Context.Groups:
                    validators = validators.FindAll(v => v is GroupValidator);
                    break;

                case Context.Validators:
                    validators = validators.FindAll(v => v is not GroupValidator and not GuardValidator);
                    break;

                case Context.Guard:
                    validators = validators.FindAll(v => v is GuardValidator);
                    break;

                default:
                    break;
            }
            validators = ApplyFilters(validators);
            return validators;
        }

        #endregion Filtering
    }
}