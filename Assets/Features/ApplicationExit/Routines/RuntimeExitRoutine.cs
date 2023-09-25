namespace MagicSwords.Features.ApplicationExit.Routines
{
    internal sealed class RuntimeExitRoutine : IApplicationExitRoutine
    {
        public void Perform()
        {
            UnityEngine.Application.Quit();
        }
    }
}
