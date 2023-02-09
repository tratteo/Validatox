using Pury.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Validatox.Editor.Settings;
using Validatox.Editor.Validators;

namespace Validatox.Editor
{
    public class Log
    {
        private object subject;

        public string Content { get; private set; }

        public string ActionIconKey { get; private set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public LogType LogType { get; private set; }

        public string ScenePath { get; private set; }

        private Log(object content)
        {
            ActionIconKey = "d_ViewToolZoom";
            Content = content.ToString();
            LogType = LogType.Log;
            subject = null;
        }

        public static LogBuilder Create(object content) => new LogBuilder(content);

        public bool TryGetSubject(out UnityEngine.Object obj)
        {
            obj = null;
            if (subject is ValidationFailure failure)
            {
                var res = failure.TryGetSubject(out obj);
                return res;
            }
            if (subject is UnityEngine.Object o)
            {
                obj = o;
                return true;
            }
            return false;
        }

        public class LogBuilder
        {
            private readonly Log log;

            public LogBuilder(object content)
            {
                log = new Log(content);
            }

            public static implicit operator Log(LogBuilder builder) => builder.Build();

            public Log Build() => log;

            /// <summary>
            ///   Define the subject that caused the log and its scene path if it is a scene object
            /// </summary>
            /// <returns> </returns>
            public LogBuilder WithSubject(object subject, string scenePath = null)
            {
                log.subject = subject;
                log.ScenePath = scenePath;
                return this;
            }

            /// <summary>
            ///   Define the icon for the log button. See <see href="https://github.com/halak/unity-editor-icons"/>
            /// </summary>
            /// <returns> </returns>
            public LogBuilder ActionIconKey(string key)
            {
                log.ActionIconKey = key;
                return this;
            }

            public LogBuilder Type(LogType logType)
            {
                log.LogType = logType;
                return this;
            }
        }
    }

    internal class LogGroup
    {
        public bool foldout = true;
        private readonly List<Log> logs;

        public Log Title { get; private set; }

        public bool DoesFold => Title != null;

        public IEnumerable<Log> Logs => logs;

        public LogGroup(Log title, IEnumerable<Log> logs)
        {
            this.Title = title;
            this.logs = new List<Log>(logs);
        }
    }

    internal class ValidatoxLogEditorWindow : PuryWindow
    {
        private static readonly string Location = $"{Environment.GetEnvironmentVariable("appdata")}{Path.AltDirectorySeparatorChar}Unity{Path.AltDirectorySeparatorChar}Validatox{Path.AltDirectorySeparatorChar}logsdb.log";
        private List<LogGroup> logGroups;
        private PurySeparator separator;
        private RectOffset defaultMargin;
        private GUIStyle logStyle;
        private GUIStyle successStyle;
        private GUIStyle failureStyle;
        private GUIStyle warningStyle;
        private GUIStyle cachedFoldoutStyle;
        private Vector2 scrollPos;

        public static void Notify(IEnumerable<ValidationFailure> failures, Log title)
        {
            var window = GetWindow<ValidatoxLogEditorWindow>(null, false);
            if (window == null) return;
            var logs = new List<Log>();
            for (int i = 0; i < failures.Count(); i++)
            {
                var failure = failures.ElementAt(i);
                var logItem = Log.Create(failure)
                    .Type(LogType.Log)
                    .ActionIconKey("d_GameObject Icon")
                    .WithSubject(failure, failure.ScenePath);
                logs.Add(logItem.Build());
            }
            window.logGroups.Add(new LogGroup(title, logs));
        }

        public static void Notify(params Log[] logs)
        {
            var window = GetWindow<ValidatoxLogEditorWindow>(null, false);
            if (window == null) return;
            foreach (var log in logs)
            {
                log.Timestamp = DateTime.Now;
            }
            window.logGroups.Add(new LogGroup(null, logs));
        }

        protected override void DrawContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var l in logGroups)
            {
                if (l.DoesFold)
                {
                    cachedFoldoutStyle = EditorStyles.foldoutHeader.Copy(a =>
                    {
                        a.richText = true;
                        a.fontSize = logStyle.fontSize + 2;
                        var c = GetStyle(l.Title.LogType).normal.textColor;
                        a.onFocused.textColor = c;
                        a.onNormal.textColor = c;
                    });

                    EditorGUILayout.BeginHorizontal();
                    if (l.Title.TryGetSubject(out var subject))
                    {
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_ViewToolZoom"),
                                      GUILayout.Width(25),
                                      GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        {
                            try
                            {
                                Selection.activeObject = subject;
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    l.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(l.foldout, l.Title.Content, cachedFoldoutStyle);
                    EditorGUILayout.EndHorizontal();
                    if (l.foldout)
                    {
                        DrawLogGroup(l);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                else
                {
                    DrawLogGroup(l);
                }

                separator.Draw();
            }
            GUILayout.EndScrollView();
        }

        protected override void Layout(List<PurySidebar> sidebars)
        {
            titleContent = new GUIContent("Log", Resources.Icon());
            ContentOrientation = Orientation.Vertical;
            ContentStyle = string.Empty;
            minSize = new Vector2(1024, 256);

            defaultMargin = new RectOffset(10, 10, 10, 10);
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(defaultMargin);

            var settings = ValidatoxSettings.Load();
            ValidatoxSettings.OnEdit += OnSettingsEdit;
            OnSettingsEdit(settings);

            sidebars.Add(PurySidebar.Factory().Top().Style("window").Draw(s =>
            {
                HorizontalGroup(() =>
                {
                    if (GUILayout.Button("Clear", GUILayout.Width(100)))
                    {
                        ClearLogs();
                    }
                    if (GUILayout.Button("Hub", GUILayout.Width(100)))
                    {
                        GetWindow<ValidatoxHubEditorWindow>();
                    }
                }, GUILayout.ExpandWidth(true));
            }));

            logGroups = new List<LogGroup>();
        }

        private void OnSettingsEdit(ValidatoxSettingsData settings)
        {
            logStyle = GUIStyle.none.Copy(s =>
            {
                s.margin = new RectOffset(10, 10, 4, 5);
                s.fontSize = settings.LogFontSize;
                s.richText = true;
            });
            logStyle.normal.textColor = settings.LogColor.HexToColor();
            failureStyle = logStyle.Copy(s => s.normal.textColor = settings.ErrorColor.HexToColor());
            successStyle = logStyle.Copy(s => s.normal.textColor = settings.SuccessColor.HexToColor());
            warningStyle = logStyle.Copy(s => s.normal.textColor = settings.WarningColor.HexToColor());
        }

        private void DrawLogGroup(LogGroup group)
        {
            VerticalGroup(() =>
            {
                foreach (var l in group.Logs)
                {
                    if (l.TryGetSubject(out var subj))
                    {
                        HorizontalGroup(() =>
                        {
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent(l.ActionIconKey),
                                  GUILayout.Width(25),
                                  GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            {
                                try
                                {
                                    if (l.ScenePath == null)
                                    {
                                        Selection.activeObject = subj;
                                        EditorGUIUtility.PingObject(subj);
                                    }
                                    else
                                    {
                                        Selection.activeObject = subj;
                                    }
                                }
                                catch (Exception)
                                { }
                            }
                            var s = GetStyle(l.LogType);
                            GUILayout.Label(l.Content, s);
                        });
                    }
                    else
                    {
                        var s = GetStyle(l.LogType);
                        GUILayout.Label(l.Content, s);
                    }
                }
            });
        }

        private void ClearLogs() => logGroups.Clear();

        private GUIStyle GetStyle(LogType type)
        {
            return type switch
            {
                LogType.Error => failureStyle,
                LogType.Assert => successStyle,
                LogType.Warning => warningStyle,
                _ => logStyle
            };
        }
    }
}