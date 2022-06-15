using System;
using UnityEngine;

namespace Validatox.Editor
{
    public static class GUIStyleExtensions
    {
        public static GUIStyle ImagePosition(this GUIStyle style, ImagePosition imagePosition)
        {
            style.imagePosition = imagePosition;
            return style;
        }

        public static GUIStyle Alignment(this GUIStyle style, TextAnchor alignment)
        {
            style.alignment = alignment;
            return style;
        }

        public static GUIStyle WordWrap(this GUIStyle style, bool wordWrap)
        {
            style.wordWrap = wordWrap;
            return style;
        }

        public static GUIStyle Clipping(this GUIStyle style, TextClipping clipping)
        {
            style.clipping = clipping;
            return style;
        }

        public static GUIStyle FontSize(this GUIStyle style, int fontSize)
        {
            style.fontSize = fontSize;
            return style;
        }

        public static GUIStyle FontStyle(this GUIStyle style, FontStyle fontStyle)
        {
            style.fontStyle = fontStyle;
            return style;
        }

        public static GUIStyle Copy(this GUIStyle style, Action<GUIStyle> factory)
        {
            var res = new GUIStyle(style);
            return res.Edit(factory);
        }

        public static GUIStyle Edit(this GUIStyle style, Action<GUIStyle> editor)
        {
            editor?.Invoke(style);
            return style;
        }
    }
}