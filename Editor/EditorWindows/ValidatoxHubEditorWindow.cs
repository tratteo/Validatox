﻿using Pury.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    internal class ValidatoxHubEditorWindow : PuryWindow
    {
        private enum ValidationFilter
        { NotValidated, Validated, All }

        private enum DirtyFilter
        { Dirty, Clean, All }

        private enum ResultFilter
        { Success, Failure, All }

        private enum Context
        { Groups, Validators, Guard }

        private PurySeparator separator;
        private GUIStyle textStyle;
        private GUIStyle titleStyle;
        private GUIStyle infoStyle;
        private Vector2 scrollPos;

        private Context context;
        private ValidationFilter validationFilter;
        private DirtyFilter dirtyFilter;
        private ResultFilter resultFilter;

        private GuardValidator guardValidator;
        private List<Validator> paresedValidators;

        protected override void Layout(List<PurySidebar> sidebars)
        {
            titleContent = new GUIContent("Validatox Hub", Resources.Icon, "Get validated :D");
            autoRepaintOnSceneChange = true;
            minSize = new Vector2(1024, 512);

            context = Context.Groups;
            validationFilter = ValidationFilter.All;
            dirtyFilter = DirtyFilter.All;
            resultFilter = ResultFilter.All;

            BuildStyles();

            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Margin(new RectOffset(10, 10, 15, 15)).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Build();

            var guid = ValidatoxSettings.Load().GuardValidatorGuid;
            guardValidator = string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<GuardValidator>(AssetDatabase.GUIDToAssetPath(guid));
            RefreshParsedValidators();
            EditorApplication.projectChanged += RefreshParsedValidators;

            sidebars.Add(PurySidebar.Factory().Left(200).Style("window").Draw(s => DrawSidebar()));
        }

        private void BuildStyles()
        {
            titleStyle = GUIStyle.none.Copy(s =>
            {
                s.fontSize = 20;
                s.alignment = TextAnchor.MiddleCenter;
                s.padding = new RectOffset(0, 0, 10, 20);
                s.normal.textColor = Color.white;
            });

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
        }

        private void OnDisable() => EditorApplication.projectChanged -= RefreshParsedValidators;

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

        private void RefreshParsedValidators() => paresedValidators = ValidatoxTools.GetAllBehavioursInAsset<Validator>();

        #region Drawing

        protected override void DrawContent()
        {
            VerticalGroup(() =>
            {
                GUILayout.Label(context.ToString(), titleStyle, GUILayout.ExpandWidth(true));
                var validators = GetValidatorsByContext();

                if (context is Context.Guard)
                {
                    guardValidator = EditorGUILayout.ObjectField("Override", guardValidator, typeof(GuardValidator), false) as GuardValidator;
                    if (!guardValidator)
                    {
                        GUILayout.Label($"Using Validatox default {nameof(GuardValidator)} located in {ValidatoxManager.PackageEditorPath}", infoStyle);
                        GUILayout.Space(5);
                    }
                    ChangeGuardValidator(guardValidator);
                    EditorGUILayout.Space(10);
                }

                HorizontalGroup(() =>
                {
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
                    if (context is Context.Guard)
                    {
                        if (GUILayout.Button("Validate", GUILayout.MaxWidth(100)))
                        {
                            ValidatoxManager.Validate();
                            GUIUtility.ExitGUI();
                        }
                    }
                });

                EditorGUILayout.Space(10);
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                foreach (var group in validators)
                {
                    DrawValidatorAsset(group);
                    separator.Draw();
                }
                GUILayout.EndScrollView();
            }, "window", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        private void DrawValidatorAsset(Validator validatorBase)
        {
            VerticalGroup(() =>
            {
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
                HorizontalGroup(() =>
                {
                    GUILayout.Label("State:", GUILayout.Width(40));
                    content = hasResult ? result.Successful ? EditorGUIUtility.TrIconContent("winbtn_mac_max") : EditorGUIUtility.TrIconContent("winbtn_mac_close") : EditorGUIUtility.TrIconContent("winbtn_mac_min");
                    GUILayout.Label(content, GUILayout.MaxWidth(20));
                    if (hasResult && !result.Successful)
                    {
                        GUILayout.Label($"{result.Failures.Count} errors");
                    }
                }, GUILayout.ExpandWidth(false));
                HorizontalGroup(() =>
                {
                    GUILayout.Label("Time:", GUILayout.Width(40));
                    content = hasResult ? EditorGUIUtility.TrTextContentWithIcon(result.Time.ToString(), "TestStopwatch") : GUIContent.none;
                    GUILayout.Label(content, GUILayout.MaxWidth(250));
                }, GUILayout.ExpandWidth(false));
                if (validatorBase is GroupValidator group)
                {
                    HorizontalGroup(() =>
                    {
                        GUILayout.Label("Size:", GUILayout.Width(40));
                        GUILayout.Label(group.GetValidators().Count.ToString(), GUILayout.MaxWidth(250));
                    }, GUILayout.ExpandWidth(false));
                }
                GUILayout.Space(10);
                HorizontalGroup(() =>
                {
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
                            var res = validatorBase.Validate((progress) => EditorUtility.DisplayProgressBar(progress.Phase, progress.Description, progress.ProgressValue));
                            var window = EditorWindow.GetWindow<ValidatoxLogEditorWindow>();
                            if (res.Successful)
                            {
                                window.NotifyLog($"{validatorBase.name} -> <color=#55d971>Validation successful! <b>:D</b></color>", LogType.Assert);
                            }
                            else
                            {
                                window.NotifyLog($"{validatorBase.name} -> <color=#ed4e4e>Validation failed with {res.Failures.Count} errors</color>\nClick for more info\n", LogType.Error);
                            }
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
                        if (EditorUtility.DisplayDialog($"Delete {validatorBase.name}", $"Are you sure you want to delete {validatorBase.name}?\n", "Yes", "Cancel"))
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(validatorBase));
                        }
                        GUIUtility.ExitGUI();
                    }
                });
            }, GUIStyle.none.Copy(s => s.margin = new RectOffset(10, 10, 10, 10)), GUILayout.ExpandWidth(true));
        }

        private void DrawSidebar()
        {
            VerticalGroup(() =>
            {
                var values = Enum.GetValues(typeof(Context));
                foreach (var value in values)
                {
                    if (GUILayout.Button(value.ToString(), GUI.skin.button.Copy(s => s.margin = new RectOffset(10, 10, 10, 10))))
                    {
                        context = (Context)value;
                    }
                }
                GUILayout.Space(25);
                GUILayout.Label("Filters", textStyle.Copy(s => s.alignment = TextAnchor.MiddleCenter), GUILayout.ExpandWidth(true));

                VerticalGroup(() =>
                {
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
                    separator.Draw();
                    if (GUILayout.Button("Clear"))
                    {
                        validationFilter = ValidationFilter.All;
                        resultFilter = ResultFilter.All;
                        dirtyFilter = DirtyFilter.All;
                    }

                    GravitateEnd(() =>
                    {
                        separator.Draw();
                        if (GUILayout.Button("Validate everything"))
                        {
                            if (EditorUtility.DisplayDialog($"Validate everything", $"Are you sure you want to run validation on the whole project?\nDepending on the size of the project this may take a while.", "Yes", "Cancel"))
                            {
                                ValidatoxManager.Validate();
                            }
                            GUIUtility.ExitGUI();
                        }
                        GUILayout.Space(5);
                    });
                }, "box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }, "window", GUILayout.MaxWidth(250), GUILayout.ExpandHeight(true));
        }

        #endregion Drawing

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