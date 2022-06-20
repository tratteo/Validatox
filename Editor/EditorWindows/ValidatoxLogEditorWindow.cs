using Newtonsoft.Json;
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
        private List<Log.Metadata> logsMeta;
        private PurySeparator separator;
        private RectOffset defaultMargin;
        private GUIStyle logStyle;
        private GUIStyle successStyle;
        private GUIStyle failureStyle;
        private Vector2 scrollPos;

        public void NotifyLog(string log, LogType type = LogType.Log)
        {
            var logItem = new Log(type, DateTime.Now.ToLongTimeString() + " | " + log);
            logs.Add(logItem);
            logsMeta.Add(logItem.Meta);
            //if (!Directory.Exists(Path.GetDirectoryName(Location)))
            //{
            //    Directory.CreateDirectory(Path.GetDirectoryName(Location));
            //}
            //Debug.Log(Environment.GetEnvironmentVariable("appdata"));
            //File.WriteAllText(Location, JsonConvert.SerializeObject(logsMeta, Formatting.Indented));
        }

        protected override void DrawContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var l in logs)
            {
                GUILayout.Label(l.Content, GetStyle(l.LogType));
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
                }, GUILayout.ExpandWidth(true));
            }));

            logsMeta = new List<Log.Metadata>();// File.Exists(Location) ? JsonConvert.DeserializeObject<List<Log.Metadata>>(File.ReadAllText(Location)) : new List<Log.Metadata>();
            logs = new List<Log>();//logsMeta.ConvertAll(m => Log.FromMetadata(m));
        }

        private void ClearLogs()
        {
            logs.Clear();
            logsMeta.Clear();
            //if (File.Exists(Location))
            //{
            //    File.Delete(Location);
            //}
        }

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

            public Metadata Meta { get; private set; }

            public Log(LogType type, string message)
            {
                var iconName = GetIconContent(type);
                LogType = type;
                Content = string.IsNullOrEmpty(iconName)
                    ? new GUIContent(message)
                    : EditorGUIUtility.TrTextContentWithIcon(message, GetIconContent(type));
                Meta = new Metadata(iconName, message, type.ToString());
            }

            public static Log FromMetadata(Metadata metadata) => new Log(Enum.Parse<LogType>(metadata.LogType), metadata.Message);

            private string GetIconContent(LogType type)
            {
                return type switch
                {
                    LogType.Error => "winbtn_mac_close",
                    LogType.Assert => "winbtn_mac_max",
                    _ => string.Empty
                };
            }

            [Serializable]
            public class Metadata
            {
                [JsonProperty("icon_name")]
                public string Icon { get; private set; }

                [JsonProperty("message")]
                public string Message { get; private set; }

                [JsonProperty("log_type")]
                public string LogType { get; private set; }

                public Metadata(string icon, string message, string logType)
                {
                    Icon = icon;
                    Message = message;
                    LogType = logType;
                }
            }
        }
    }
}