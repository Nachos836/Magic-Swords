using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace MagicSwords.Features.Input
{
    public readonly struct UISubmission : IInputAcquire { }

    internal sealed class UI : IAsyncStartable, IDisposable, IInputFor<UISubmission>
    {
        private readonly IUIActionsProvider _uiActions;
        private readonly PlayerLoopTiming _initializationPoint;

        private readonly InputAction _pointer;
        private readonly InputAction _click;
        private readonly InputAction _submit;
        private readonly InputAction _back;

        public UI(IUIActionsProvider uiActions, PlayerLoopTiming initializationPoint)
        {
            _initializationPoint = initializationPoint;
            _uiActions = uiActions;
            _pointer = _uiActions.Get().Pointer;
            _click = _uiActions.Get().Click;
            _submit = _uiActions.Get().Submit;
            _back = _uiActions.Get().Back;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _uiActions.Get().Enable();
            _pointer.Enable();
            _click.Enable();
            _submit.Enable();
            _back.Enable();
        }

        void IDisposable.Dispose()
        {
            _back.Disable();
            _submit.Disable();
            _click.Disable();
            _pointer.Disable();
            _uiActions.Get().Disable();

            _pointer.Dispose();
            _click.Dispose();
            _submit.Dispose();
            _back.Dispose();
        }

        IDisposable IInputFor<UISubmission>.Subscribe
        (
            Action<InputContext> started,
            Action<InputContext> performed,
            Action<InputContext> canceled
        ) {
            return new InputSubscription(_submit, started, performed, canceled)
                .Subscribe();
        }
    }
}
