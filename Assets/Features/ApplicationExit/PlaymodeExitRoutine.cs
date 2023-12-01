namespace MagicSwords.Features.ApplicationExit
{
    internal sealed class ApplicationExitRoutine : Internal.ExitRoutine, IApplicationExitRoutine
    {
    }

    public interface IApplicationExitRoutine
    {
        void Perform();
    }
}
