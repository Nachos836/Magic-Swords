using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.Actions.Handling
{
    internal sealed class InputSubscription
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

            return new UnsubscribeHandler(() =>
            {
                _input.canceled -= _canceled;
                _input.performed -= _performed;
                _input.started -= _started;
            });
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly Action _unsubscribe;

            public UnsubscribeHandler(Action unsubscribe) => _unsubscribe = unsubscribe;

            void IDisposable.Dispose() => _unsubscribe.Invoke();
        }
    }
}
