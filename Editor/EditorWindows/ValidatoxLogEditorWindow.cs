using Pury.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class ValidatoxLogEditorWindow : PuryWindow
    {
        private static readonly string Location = $"{Environment.GetEnvironmentVariable("appdata")}{Path.AltDirectorySeparatorChar}Unity{Path.AltDirectorySeparatorChar}Validatox{Path.AltDirectorySeparatorChar}logsdb.log";
        private List<Log> logs;
        private PurySeparator separator;
        private RectOffset defaultMargin;
        private GUIStyle logStyle;
        private GUIStyle successStyle;
        private GUIStyle failureStyle;
        private Vector2 scrollPos;

        public static void NotifyLog(string log, LogType type = LogType.Log, UnityEngine.Object subject = null)
        {
            var window = GetWindow<ValidatoxLogEditorWindow>(null, false);
            if (window is null) return;
            var logItem = new Log(type, DateTime.Now.ToLongTimeString() + " | " + log, subject);
            window.logs.Add(logItem);
        }

        protected override void DrawContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var l in logs)
            {
                HorizontalGroup(() =>
                {
                    if (l.Subject)
                    {
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_ViewToolZoom"), GUILayout.Width(25), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        {
                            Selection.activeObject = l.Subject;
                        }
                    }
                    GUILayout.Label(l.Content, GetStyle(l.LogType));
                });
                separator.Draw();
            }
            GUILayout.EndScrollView();
        }

        protected override void Layout(List<PurySidebar> sidebars)
        {
            titleContent = new GUIContent("Log", Resources.Icon, ":D");
            ContentOrientation = Orientation.Vertical;
            ContentStyle = string.Empty;
            minSize = new Vector2(1024, 256);
            defaultMargin = new RectOffset(10, 10, 10, 10);
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(defaultMargin);

            logStyle = GUIStyle.none.Copy(s =>
            {
                s.margin = defaultMargin;
                s.fontSize = 12;
                s.richText = true;
            });
            logStyle.normal.textColor = Color.white;

            failureStyle = logStyle.Copy(s =>
            {
                s.normal.textColor = Color.red;
                s.fontSize = 14;
            });
            successStyle = logStyle.Copy(s =>
            {
                s.normal.textColor = Color.green;
                s.fontSize = 12;
            });

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

            logs = new List<Log>();
        }

        private void ClearLogs() => logs.Clear();

        private GUIStyle GetStyle(LogType type)
        {
            return type switch
            {
                LogType.Error => failureStyle,
                LogType.Assert => successStyle,
                _ => logStyle
            };
        }

        private class Log
        {
            public GUIContent Content { get; private set; }

            public LogType LogType { get; private set; }

            public UnityEngine.Object Subject { get; private set; }

            public Log(LogType type, string message, UnityEngine.Object subject)
            {
                var iconName = GetIconContent(type);
                LogType = type;
                Subject = subject;
                Content = string.IsNullOrEmpty(iconName)
                    ? new GUIContent(message)
                    : EditorGUIUtility.TrTextContentWithIcon(message, GetIconContent(type));
            }

            private string GetIconContent(LogType type)
            {
                return type switch
                {
                    LogType.Error => "winbtn_mac_close",
                    LogType.Assert => "winbtn_mac_max",
                    _ => string.Empty
                };
            }
        }
    }
}