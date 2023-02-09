using Pury.Editor;
using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    /// <summary>
    ///   Create a fix composed of multiple fixes that are all rendered in the popup window. Eligible for automatic fixing: the fix are
    ///   applied depending on children fixes
    /// </summary>
    /// <typeparam name="T1"> </typeparam>
    /// <typeparam name="T2"> </typeparam>
    public class CompositeFix<T1, T2> : ValidationFix where T1 : ValidationFix where T2 : ValidationFix
    {
        private readonly T1 first;
        private readonly T2 second;
        private readonly PurySeparator separator;

        public CompositeFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            first = InstantiateFix(typeof(T1), failure, args) as T1;
            second = InstantiateFix(typeof(T2), failure, args) as T2;
            ContextlessFix = first.ContextlessFix || second.ContextlessFix;
            Size += Vector2.one * 100;
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(new RectOffset(5, 5, 5, 5));
        }

        public override string GetLabel() => $"{nameof(CompositeFix<T1, T2>)}<{first.GetLabel()},{second.GetLabel()}>";

        protected override bool Fix(SerializedObject serializedObject)
        {
            first.ApplyFix();
            second.ApplyFix();
            return false;
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            base.EditorRender(window);
            EditorGUILayout.BeginVertical();
            separator.Draw();
            first.Render(window);
            separator.Draw();
            second.Render(window);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    ///   <inheritdoc cref="CompositeFix{T1, T2}"/>
    /// </summary>
    public class CompositeFix<T1, T2, T3> : ValidationFix where T1 : ValidationFix where T2 : ValidationFix where T3 : ValidationFix
    {
        private readonly T1 first;
        private readonly T2 second;
        private readonly T3 third;
        private readonly PurySeparator separator;

        public CompositeFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            first = InstantiateFix(typeof(T1), failure, args) as T1;
            second = InstantiateFix(typeof(T2), failure, args) as T2;
            third = InstantiateFix(typeof(T3), failure, args) as T3;
            ContextlessFix = first.ContextlessFix || second.ContextlessFix || third.ContextlessFix;
            Size += Vector2.one * 150;
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(new RectOffset(5, 5, 5, 5));
        }

        public override string GetLabel() => $"{nameof(CompositeFix<T1, T2, T3>)}<{first.GetLabel()}, {second.GetLabel()}, {third.GetLabel()}>";

        protected override bool Fix(SerializedObject serializedObject)
        {
            first.ApplyFix();
            second.ApplyFix();
            third.ApplyFix();
            return false;
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            base.EditorRender(window);
            EditorGUILayout.BeginVertical();
            separator.Draw();
            first.Render(window);
            separator.Draw();
            second.Render(window);
            separator.Draw();
            third.Render(window);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    ///   <inheritdoc cref="CompositeFix{T1, T2}"/>
    /// </summary>
    public class CompositeFix<T1, T2, T3, T4> : ValidationFix
        where T1 : ValidationFix
        where T2 : ValidationFix
        where T3 : ValidationFix
        where T4 : ValidationFix
    {
        private readonly T1 first;
        private readonly T2 second;
        private readonly T3 third;
        private readonly T4 fourth;
        private readonly PurySeparator separator;

        public CompositeFix(ValidationFailure failure, params object[] args) : base(failure, args)
        {
            first = InstantiateFix(typeof(T1), failure, args) as T1;
            second = InstantiateFix(typeof(T2), failure, args) as T2;
            third = InstantiateFix(typeof(T3), failure, args) as T3;
            fourth = InstantiateFix(typeof(T4), failure, args) as T4;
            ContextlessFix = first.ContextlessFix || second.ContextlessFix || third.ContextlessFix || fourth.ContextlessFix;
            Size += Vector2.one * 200;
            separator = PurySeparator.Towards(Orientation.Horizontal).Thickness(1).Colored(new Color(0.5F, 0.5F, 0.5F, 1)).Margin(new RectOffset(5, 5, 5, 5));
        }

        public override string GetLabel() => $"{nameof(CompositeFix<T1, T2, T3, T4>)}<{first.GetLabel()}, {second.GetLabel()}, {third.GetLabel()}, {fourth.GetLabel()}>";

        protected override bool Fix(SerializedObject serializedObject)
        {
            first.ApplyFix();
            second.ApplyFix();
            third.ApplyFix();
            fourth.ApplyFix();
            return false;
        }

        protected override void EditorRender(ValidationFixWindow window)
        {
            base.EditorRender(window);
            EditorGUILayout.BeginVertical();
            separator.Draw();
            first.Render(window);
            separator.Draw();
            second.Render(window);
            separator.Draw();
            third.Render(window);
            separator.Draw();
            fourth.Render(window);
            EditorGUILayout.EndVertical();
        }
    }
}