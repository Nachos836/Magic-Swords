using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace MagicSwords.Features.Input.Actions
{
    using PlayerDriven;
    using Handling;

    internal sealed class Reading : IAsyncStartable, IDisposable, IInputFor<ReadingSkip>
    {
        private readonly IReadingActionsProvider _readingActions;
        private readonly PlayerLoopTiming _initializationPoint;

        private readonly InputAction _skip;

        public Reading(IReadingActionsProvider readingActions, PlayerLoopTiming initializationPoint)
        {
            _readingActions = readingActions;
            _initializationPoint = initializationPoint;

            _skip = readingActions.Get().Skip;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                    .SuppressCancellationThrow()) return;

            _readingActions.Get().Enable();
            _skip.Enable();
        }

        void IDisposable.Dispose()
        {
            _skip.Disable();
            _readingActions.Get().Disable();

            _skip.Dispose();
        }

        IDisposable IInputFor<ReadingSkip>.Subscribe(Action<StartedContext> started)
        {
            return new StartedSubscription(_skip, started)
                .Subscribe();
        }

        IDisposable IInputFor<ReadingSkip>.Subscribe<T>(T target, Action<T, StartedContext> started)
        {
            return new StartedSubscription<T>(target, _skip, started)
                .Subscribe();
        }

        IDisposable IInputFor<ReadingSkip>.Subscribe(Action<PerformedContext> performed)
        {
            return new PerformedSubscription(_skip, performed)
                .Subscribe();
        }

        IDisposable IInputFor<ReadingSkip>.Subscribe(Action<CanceledContext> canceled)
        {
            return new CanceledSubscription(_skip, canceled)
                .Subscribe();
        }

        IDisposable IInputFor<ReadingSkip>.Subscribe
        (
            Action<StartedContext> started,
            Action<PerformedContext> performed,
            Action<CanceledContext> canceled
        ) {
            return new InputSubscription(_skip, started, performed, canceled)
                .Subscribe();
        }
    }
}
