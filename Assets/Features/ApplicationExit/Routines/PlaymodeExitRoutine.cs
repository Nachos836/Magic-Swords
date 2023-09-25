using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MagicSwords.Features.ApplicationExit.Routines
{
    internal sealed class PlaymodeExitRoutine : IApplicationExitRoutine
    {
        CancellationToken IApplicationExitRoutine.CancellationToken { get; } = Application.exitCancellationToken;

        void IApplicationExitRoutine.Perform()
        {
#       if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#       endif
        }
    }
}
