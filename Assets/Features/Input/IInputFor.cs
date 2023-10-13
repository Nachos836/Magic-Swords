using System;

namespace MagicSwords.Features.Input
{
    // ReSharper disable once UnusedTypeParameter
    // Added for a sake of Ad-hoc polymorphism
    public interface IInputFor<TInputAcquire> where TInputAcquire : unmanaged, IInputAcquire
    {
        IDisposable Subscribe(Action<StartedContext> started);
        IDisposable Subscribe(Action<PerformedContext> performed);
        IDisposable Subscribe(Action<CanceledContext> canceled);

        IDisposable Subscribe
        (
            Action<StartedContext> started,
            Action<PerformedContext> performed,
            Action<CanceledContext> canceled
        );
    }

    public interface IInputAcquire { }
    public readonly struct UISubmission : IInputAcquire { }
    public readonly struct ReadingSkip : IInputAcquire { }

    public readonly struct StartedContext
    {
        internal static StartedContext Empty { get; } = new ();

        // Add fields and stuff when needed
    }

    public readonly struct CanceledContext
    {
        internal static CanceledContext Empty { get; } = new ();

        // Add fields and stuff when needed
    }

    public readonly struct PerformedContext
    {
        internal static PerformedContext Empty { get; } = new ();

        // Add fields and stuff when needed
    }
}
