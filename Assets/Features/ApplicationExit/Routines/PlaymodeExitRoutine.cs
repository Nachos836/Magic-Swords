using UnityEditor;

namespace MagicSwords.Features.ApplicationExit.Routines
{
    internal sealed class PlaymodeExitRoutine : IApplicationExitRoutine
    {
        public void Perform()
        {
#       if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#       endif
        }
    }
}
