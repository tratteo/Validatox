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
            window.titleContent = new GUIContent(fix.Title);
            window.minSize = fix.Size;
            window.maxSize = fix.Size;
            var r = window.position;
            r.x = options.Position.x;
            r.y = options.Position.y;
            window.position = r;
        }

        private void HandleClose()
        {
            Close();
        }

        private void OnGUI()
        {
            try
            {
                minSize = fix.Size;
                maxSize = fix.Size;
                separator ??= PurySeparator.Towards(Orientation.Horizontal).Margin(new RectOffset(8, 8, 8, 8));
                float padding = 10;
                Rect area = new Rect(padding, padding,
                     position.width - padding * 2f, position.height - padding * 2f);

                GUILayout.BeginArea(area);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                if (fix == null) HandleClose();
                fix.Render(this);
                separator.Draw();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply"))
                {
                    fix.ApplyFix();
                    //HandleClose();
                }
                if (GUILayout.Button("Close"))
                {
                    HandleClose();
                }
                EditorGUILayout.EndHorizontal();
            }
            catch (Exception e)
            {
                if (e is not ExitGUIException)
                {
                    Debug.LogWarning("Unexpected error in drawing fix window");
                    HandleClose();
                }
            }
            finally
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
    }
}