using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.Actions.Handling
{
    internal readonly ref struct PerformedSubscription
    {
        private readonly InputAction _input;
        private readonly Action<PerformedContext> _performed;

        public PerformedSubscription(InputAction input, Action<PerformedContext> performed)
        {
            _input = input;
            _performed = performed;
        }

        public IDisposable Subscribe()
        {
            var inputAction = _performed;
            Action<InputAction.CallbackContext> callback = _ => inputAction.Invoke(PerformedContext.Empty);

            _input.performed += callback;
            return new UnsubscribeHandler(_input, callback);
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _performed;

            public UnsubscribeHandler(InputAction input, Action<InputAction.CallbackContext> performed)
            {
                _input = input;
                _performed = performed;
            }

            void IDisposable.Dispose() => _input.performed -= _performed;
        }
    }
}
