using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.Actions.Handling
{
    internal readonly ref struct InputSubscription
    {
        private readonly InputAction _input;
        private readonly Action<StartedContext> _started;
        private readonly Action<PerformedContext> _performed;
        private readonly Action<CanceledContext> _canceled;

        public InputSubscription
        (
            InputAction input,
            Action<StartedContext> started,
            Action<PerformedContext> performed,
            Action<CanceledContext> canceled
        ) {
            _input = input;
            _started = started;
            _performed = performed;
            _canceled = canceled;
        }

        public IDisposable Subscribe()
        {
            var startedAction = _started;
            var performedAction = _performed;
            var canceledAction = _canceled;
            Action<InputAction.CallbackContext> startedCallback = _ => startedAction.Invoke(StartedContext.Empty);
            Action<InputAction.CallbackContext> performedCallback = _ => performedAction.Invoke(PerformedContext.Empty);
            Action<InputAction.CallbackContext> canceledCallback = _ => canceledAction.Invoke(CanceledContext.Empty);

            _input.started += startedCallback;
            _input.performed += performedCallback;
            _input.canceled += canceledCallback;

            return new UnsubscribeHandler(_input, startedCallback, performedCallback, canceledCallback);
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _started;
            private readonly Action<InputAction.CallbackContext> _performed;
            private readonly Action<InputAction.CallbackContext> _canceled;

            public UnsubscribeHandler
            (
                InputAction input,
                Action<InputAction.CallbackContext> started,
                Action<InputAction.CallbackContext> performed,
                Action<InputAction.CallbackContext> canceled
            ) {
                _input = input;
                _started = started;
                _performed = performed;
                _canceled = canceled;
            }

            void IDisposable.Dispose()
            {
                _input.canceled -= _started;
                _input.performed -= _performed;
                _input.started -= _canceled;
            }
        }
    }
}
