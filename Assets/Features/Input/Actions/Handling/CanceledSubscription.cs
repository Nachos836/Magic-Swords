using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.Actions.Handling
{
    internal sealed class CanceledSubscription
    {
        private readonly InputAction _input;
        private readonly Action<CanceledContext> _canceled;

        public CanceledSubscription(InputAction input, Action<CanceledContext> canceled)
        {
            _input = input;
            _canceled = canceled;
        }

        public IDisposable Subscribe()
        {
            var inputAction = _canceled;
            Action<InputAction.CallbackContext> callback = _ => inputAction.Invoke(CanceledContext.Empty);

            _input.canceled += callback;
            return new UnsubscribeHandler(_input, callback);
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _canceled;

            public UnsubscribeHandler(InputAction input, Action<InputAction.CallbackContext> canceled)
            {
                _input = input;
                _canceled = canceled;
            }

            void IDisposable.Dispose() => _input.canceled -= _canceled;
        }
    }
}
