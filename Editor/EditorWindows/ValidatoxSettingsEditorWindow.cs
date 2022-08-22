using Pury.Editor;
using System.Collections.Generic;
using UnityEngine;
using Validatox.Editor.Settings;

namespace Validatox.Editor
{
    internal class ValidatoxSettingsEditorWindow : PuryWindow
    {
        private enum Context
        { BuildPipeline }

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
            var dirty = false;
            switch (context)
            {
                case Context.BuildPipeline:
                    VerticalGroup(() =>
                    {
                        var secure = GUILayout.Toggle(settings.SecureBuildPipeline, "Secure build pipeline");
                        GUILayout.Label("Enable the secure build pipeline. Upon build, run the validation and interrupt the build in case of failed validation", descriptionStyle);
                        if (secure != settings.SecureBuildPipeline)
                        {
                            dirty = true;
                            settings.SecureBuildPipeline = secure;
                        }

                        if (secure)
                        {
                            separator.Draw();
                            GUILayout.Label("Select what to validate upon building", textStyle);
                            VerticalGroup(() =>
                            {
                                var bvs = GUILayout.Toggle(settings.BuildValidate, new GUIContent("Validators", "Enable the validation of all singles and group validators in the project"), singleStyle);
                                if (bvs != settings.BuildValidate)
                                {
                                    dirty = true;
                                    settings.BuildValidate = bvs;
                                }

                                var bvgu = GUILayout.Toggle(settings.BuildValidateGuards, new GUIContent("Guards", "Enable the validation of the guarding framework using the assigned GuardValidator"), singleStyle);
                                if (bvgu != settings.BuildValidateGuards)
                                {
                                    dirty = true;
                                    settings.BuildValidateGuards = bvgu;
                                }
                            });
                        }
                    }, groupStyle);
                    break;

                default:
                    break;
            }
            if (dirty)
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
            }, "window", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
    }
}