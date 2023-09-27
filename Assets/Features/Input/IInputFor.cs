using System;

namespace MagicSwords.Features.Input
{
    // ReSharper disable once UnusedTypeParameter
    // Added for a sake of Ad-hoc polymorphism
    public interface IInputFor<TInputAcquire> where TInputAcquire : unmanaged, IInputAcquire
    {
        IDisposable Subscribe
        (
            Action<InputContext> started,
            Action<InputContext> performed,
            Action<InputContext> canceled
        );
    }

    public interface IInputAcquire { }
    public readonly struct UISubmission : IInputAcquire { }
    public readonly struct ReadingSkip : IInputAcquire { }

    public readonly struct InputContext
    {
        // Add fields and stuff when needed
    }
}
