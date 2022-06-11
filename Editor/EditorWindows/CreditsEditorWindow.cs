using UnityEditor;
using UnityEngine;

namespace Validatox.Editor
{
    internal class CreditsEditorWindow : EditorWindow
    {
        private GUIStyle titleStyle;
        private GUIStyle textStyle;

        private void OnEnable()
        {
            maxSize = new Vector2(400, 512);
            minSize = new Vector2(400, 512);
            titleStyle = new GUIStyle()
            {
                fontSize = 20,
                richText = true,
                clipping = TextClipping.Clip,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove
            };
            titleStyle.normal.textColor = new Color(188 / 255F, 210 / 255F, 238 / 255F);

            textStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                richText = true,
                fontSize = 14
            };
            textStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label(new GUIContent() { image = Resources.Logo }, titleStyle, GUILayout.Height(200));
            GUILayout.Label(new GUIContent() { image = Resources.Title, text = "\nBy <i>Siamango</i>" }, titleStyle, GUILayout.Height(128));
        }
    }
}