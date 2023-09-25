using System.Threading;

namespace MagicSwords.Features.ApplicationExit
{
    public interface IApplicationExitRoutine
    {
        void Perform();
        CancellationToken CancellationToken { get; }
    }
}
