using Pury.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;

namespace Validatox.Editor
{
    internal class ValidatoxSettingsEditorWindow : PuryWindow
    {
        private enum Context
        { BuildPipeline, Appearance }

        private Context context;
        private PurySeparator separator;
        private GUIStyle descriptionStyle;
        private GUIStyle textStyle;
        private ValidatoxSettingsData settings;

        protected override void Layout(List<PurySidebar> sidebars)
        {
            titleContent = new GUIContent("Settings", Resources.Icon());
            minSize = new Vector2(512, 256);
            context = Context.BuildPipeline;
            BuildStyles();
            settings = ValidatoxSettings.Load();
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Margin(new RectOffset(10, 10, 10, 10)).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Build();
            sidebars.Add(PurySidebar.Factory().Left(200).Style("window").Draw(DrawSidebar));
        }

        protected override void DrawContent()
        {
            var singleStyle = GUI.skin.toggle.Copy(s => s.margin = new RectOffset(10, 10, 10, 10));
            var groupStyle = GUI.skin.box.Copy(s => s.margin = new RectOffset(10, 10, 10, 10));
            EditorGUI.BeginChangeCheck();
            switch (context)
            {
                case Context.BuildPipeline:
                    VerticalGroup(() =>
                    {
                        settings.SecureBuildPipeline = GUILayout.Toggle(settings.SecureBuildPipeline, "Secure build pipeline");
                        GUILayout.Label("Enable the secure build pipeline. Upon build, run the validation and interrupt the build in case of failed validation", descriptionStyle);

                        if (settings.SecureBuildPipeline)
                        {
                            separator.Draw();
                            GUILayout.Label("Select what to validate upon building", textStyle);
                            VerticalGroup(() =>
                            {
                                settings.BuildValidate = GUILayout.Toggle(settings.BuildValidate, new GUIContent("Validators", "Enable the validation of all singles and group validators in the project"), singleStyle);
                                settings.BuildValidateGuards = GUILayout.Toggle(settings.BuildValidateGuards, new GUIContent("Guards", "Enable the validation of the guarding framework using the assigned GuardValidator"), singleStyle);
                            });
                        }
                    }, groupStyle);
                    break;

                case Context.Appearance:
                    VerticalGroup(() =>
                    {
                        EditorGUILayout.LabelField("Log colors", textStyle);
                        settings.LogColor = EditorGUILayout.ColorField("Info", settings.LogColor.HexToColor()).ToHex();
                        settings.SuccessColor = EditorGUILayout.ColorField("Success", settings.SuccessColor.HexToColor()).ToHex();
                        settings.WarningColor = EditorGUILayout.ColorField("Warning", settings.WarningColor.HexToColor()).ToHex();
                        settings.ErrorColor = EditorGUILayout.ColorField("Error", settings.ErrorColor.HexToColor()).ToHex();
                        separator.Draw();
                        EditorGUILayout.LabelField("Font", textStyle);
                        settings.LogFontSize = EditorGUILayout.IntSlider("Size", settings.LogFontSize, 8, 20);
                        GravitateEnd(() =>
                        {
                            if (GUILayout.Button("Reset"))
                            {
                                settings.LogColor = ValidatoxSettingsData.DefaultLogColor;
                                settings.SuccessColor = ValidatoxSettingsData.DefaultSuccessColor;
                                settings.WarningColor = ValidatoxSettingsData.DefaultWarningColor;
                                settings.ErrorColor = ValidatoxSettingsData.DefaultErrorColor;
                                settings.LogFontSize = ValidatoxSettingsData.DefaultFontSize;
                            }
                        });
                    }, groupStyle);
                    break;

                default:
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                ValidatoxSettings.Save(settings);
            }
        }

        private void BuildStyles()
        {
            descriptionStyle = GUIStyle.none.Copy(s =>
            {
                s.wordWrap = true;
                s.fontStyle = FontStyle.Italic;
                s.fontSize = 10;
                s.normal.textColor = new Color(0.65F, 0.65F, 0.65F, 1);
                s.margin = new RectOffset(5, 5, 5, 5);
            });

            textStyle = GUIStyle.none.Copy(s =>
            {
                s.imagePosition = ImagePosition.ImageLeft;
                s.alignment = TextAnchor.MiddleLeft;
                s.margin = new RectOffset(10, 10, 0, 0);
                s.fontSize = 12;
                s.normal.textColor = new Color(0.85F, 0.85F, 0.85F, 1);
            });
        }

        private void DrawSidebar(PurySidebar sidebar)
        {
            VerticalGroup(() =>
            {
                if (GUILayout.Button("Build pipeline", GUI.skin.button.Copy(s => s.margin = new RectOffset(10, 10, 10, 10))))
                {
                    context = Context.BuildPipeline;
                }
                if (GUILayout.Button("Appearance", GUI.skin.button.Copy(s => s.margin = new RectOffset(10, 10, 10, 10))))
                {
                    context = Context.Appearance;
                }
            }, "window", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
    }
}