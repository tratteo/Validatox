using Pury.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Validatox.Editor
{
    internal class CreditsEditorWindow : PuryWindow
    {
        private GUIStyle titleStyle;

        protected override void Layout(List<PurySidebar> sidebars)
        {
            maxSize = new Vector2(400, 512);
            minSize = new Vector2(400, 512);
            titleStyle = new GUIStyle()
            {
                fontSize = 20,
                richText = true
            };
            titleStyle.normal.textColor = new Color(205 / 255F, 205 / 255F, 205 / 255F);

            ContentOrientation = Orientation.Vertical;
        }

        protected override void DrawContent()
        {
            GUILayout.BeginHorizontal();
            Center(() => GUILayout.Label(new GUIContent() { image = Resources.Logo }, GUILayout.Width(250), GUILayout.Height(250)));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Center(() => GUILayout.Label("\nBy <i>Siamango</i>", titleStyle));
            GUILayout.EndHorizontal();
        }
    }
}