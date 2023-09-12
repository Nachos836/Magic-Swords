using System;
using UnityEditor;
using UnityEngine.Assertions;

using static System.Reflection.BindingFlags;

namespace MagicSwords.Features.UnityEditorUtils
{
    public static class EditorFocusHandling
    {
        private const string FocusChanged = "focusChanged";

        private static readonly Type Editor = typeof(EditorApplication);

        public static Action<bool> UnityEditorFocusChanged
        {
            get
            {
                var fieldInfo = Editor.GetField(FocusChanged,Static | NonPublic);

                Assert.IsNotNull(fieldInfo);

                return (Action<bool>) fieldInfo.GetValue(null);
            }
            set
            {
                var fieldInfo = Editor.GetField(FocusChanged,Static | NonPublic);

                Assert.IsNotNull(fieldInfo);

                fieldInfo.SetValue(null, value);
            }
        }
    }
}