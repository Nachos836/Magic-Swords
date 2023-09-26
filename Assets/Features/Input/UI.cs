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

        private sealed class InputSubscription
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _started;
            private readonly Action<InputAction.CallbackContext> _performed;
            private readonly Action<InputAction.CallbackContext> _canceled;

            public InputSubscription
            (
                InputAction input,
                Action<InputContext> started,
                Action<InputContext> performed,
                Action<InputContext> canceled
            ) {
                _input = input;
                _started = _ => started.Invoke(new InputContext());
                _performed = _ => performed.Invoke(new InputContext());
                _canceled = _ => canceled.Invoke(new InputContext());
            }

            public IDisposable Subscribe()
            {
                _input.started += _started;
                _input.performed += _performed;
                _input.canceled += _canceled;

                return new Handler(() =>
                {
                    _input.canceled -= _canceled;
                    _input.performed -= _performed;
                    _input.started -= _started;
                });
            }

            private sealed class Handler : IDisposable
            {
                private readonly Action _unsubscribe;

                public Handler(Action unsubscribe) => _unsubscribe = unsubscribe;

                void IDisposable.Dispose() => _unsubscribe.Invoke();
            }
        }
    }
}
