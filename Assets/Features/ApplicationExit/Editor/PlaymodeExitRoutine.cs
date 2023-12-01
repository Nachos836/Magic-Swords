using UnityEditor;

// ReSharper disable once CheckNamespace
namespace MagicSwords.Features.ApplicationExit.Internal
{
    public class ExitRoutine
    {
        public void Perform() => EditorApplication.ExitPlaymode();
    }
}
