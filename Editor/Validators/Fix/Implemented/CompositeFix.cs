using UnityEditor;
using UnityEngine;

namespace Validatox.Editor.Validators.Fix
{
    public class CompositeFix<T1, T2> : ValidationFix where T1 : ValidationFix where T2 : ValidationFix
    {
        private readonly T1 first;
        private readonly T2 second;

        public CompositeFix(Validator validator, Object subject, params object[] args) : base(validator, subject, args)
        {
            first = InstantiateFix(typeof(T1), validator, subject, args) as T1;
            second = InstantiateFix(typeof(T2), validator, subject, args) as T2;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            EditorGUILayout.BeginVertical();
            first.EditorRender(window);
            EditorGUILayout.Space(4);
            second.EditorRender(window);
            EditorGUILayout.EndVertical();
        }
    }

    public class CompositeFix<T1, T2, T3> : ValidationFix where T1 : ValidationFix where T2 : ValidationFix where T3 : ValidationFix
    {
        private readonly T1 first;
        private readonly T2 second;
        private readonly T3 third;

        public CompositeFix(Validator validator, Object subject, params object[] args) : base(validator, subject, args)
        {
            first = InstantiateFix(typeof(T1), validator, subject, args) as T1;
            second = InstantiateFix(typeof(T2), validator, subject, args) as T2;
            third = InstantiateFix(typeof(T3), validator, subject, args) as T3;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            EditorGUILayout.BeginVertical();
            first.EditorRender(window);
            EditorGUILayout.Space(4);
            second.EditorRender(window);
            EditorGUILayout.Space(4);
            third.EditorRender(window);
            EditorGUILayout.EndVertical();
        }
    }

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

        public CompositeFix(Validator validator, Object subject, params object[] args) : base(validator, subject, args)
        {
            first = InstantiateFix(typeof(T1), validator, subject, args) as T1;
            second = InstantiateFix(typeof(T2), validator, subject, args) as T2;
            third = InstantiateFix(typeof(T3), validator, subject, args) as T3;
            fourth = InstantiateFix(typeof(T4), validator, subject, args) as T4;
        }

        public override void EditorRender(ValidationFixWindow window)
        {
            EditorGUILayout.BeginVertical();
            first.EditorRender(window);
            EditorGUILayout.Space(4);
            second.EditorRender(window);
            EditorGUILayout.Space(4);
            third.EditorRender(window);
            EditorGUILayout.Space(4);
            fourth.EditorRender(window);
            EditorGUILayout.EndVertical();
        }
    }
}