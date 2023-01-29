using Pury.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class ValidationFixWindowOptions
    {
        public Vector2 Position { get; set; } = Vector2.zero;
    }

    public class ValidationFixWindow : EditorWindow
    {
        private PurySeparator separator;
        private ValidationFix fix;
        private Vector2 scrollPos;

        public static void Open(ValidationFix fix, ValidationFixWindowOptions options = null)
        {
            ValidationFixWindow window = CreateInstance<ValidationFixWindow>();
            window.fix = fix;
            switch (fix.Style)
            {
                case ValidationFix.OpenStyle.Popup:
                    window.ShowPopup();
                    break;

                case ValidationFix.OpenStyle.Utility:
                    window.ShowUtility();
                    break;

                case ValidationFix.OpenStyle.Normal:
                    window.Show();
                    break;

                default:
                    window.ShowPopup();
                    break;
            }
            window.minSize = fix.Size;
            window.maxSize = fix.Size;
            window.titleContent = new GUIContent(fix.Title);
            var r = window.position;
            r.x = options.Position.x;
            r.y = options.Position.y;
            window.position = r;
        }

        private void OnGUI()
        {
            separator ??= PurySeparator.Towards(Orientation.Horizontal).Margin(new RectOffset(8, 8, 8, 8));
            float padding = 10;
            Rect area = new Rect(padding, padding,
                 position.width - padding * 2f, position.height - padding * 2f);

            GUILayout.BeginArea(area);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            try
            {
                if (fix == null) Close();
                fix.EditorRender(this);
                GUILayout.FlexibleSpace();
                separator.Draw();
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Close"))
                {
                    Close();
                }
                EditorGUILayout.EndHorizontal();
            }
            catch (Exception)
            {
                Close();
            }
            finally
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
    }
}