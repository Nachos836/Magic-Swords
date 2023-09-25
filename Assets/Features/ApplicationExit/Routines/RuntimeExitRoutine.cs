using System.Threading;
using UnityEngine;

namespace MagicSwords.Features.ApplicationExit.Routines
{
    internal sealed class RuntimeExitRoutine : IApplicationExitRoutine
    {
        CancellationToken IApplicationExitRoutine.CancellationToken { get; } = Application.exitCancellationToken;

        void IApplicationExitRoutine.Perform() => Application.Quit();
    }
}
