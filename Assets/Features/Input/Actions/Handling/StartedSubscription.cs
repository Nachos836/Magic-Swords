using System;
using UnityEngine.InputSystem;

namespace MagicSwords.Features.Input.Actions.Handling
{
    internal readonly ref struct StartedSubscription
    {
        private readonly InputAction _input;
        private readonly Action<StartedContext> _started;

        public StartedSubscription(InputAction input, Action<StartedContext> started)
        {
            _input = input;
            _started = started;
        }

        public IDisposable Subscribe()
        {
            var inputAction = _started;
            Action<InputAction.CallbackContext> callback = _ => inputAction.Invoke(StartedContext.Empty);

            _input.started += callback;
            return new UnsubscribeHandler(_input, callback);
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _started;

            public UnsubscribeHandler(InputAction input, Action<InputAction.CallbackContext> started)
            {
                _input = input;
                _started = started;
            }

            void IDisposable.Dispose() => _input.started -= _started;
        }
    }

    internal readonly ref struct StartedSubscription<T>
    {
        private readonly T _target;
        private readonly InputAction _input;
        private readonly Action<T, StartedContext> _started;

        public StartedSubscription(T target, InputAction input, Action<T, StartedContext> started)
        {
            _target = target;
            _input = input;
            _started = started;
        }

        public IDisposable Subscribe()
        {
            var inputAction = _started;
            var target = _target;
            Action<InputAction.CallbackContext> callback = _ => inputAction.Invoke(target, StartedContext.Empty);

            _input.started += callback;
            return new UnsubscribeHandler(_input, callback);
        }

        private sealed class UnsubscribeHandler : IDisposable
        {
            private readonly InputAction _input;
            private readonly Action<InputAction.CallbackContext> _started;

            public UnsubscribeHandler(InputAction input, Action<InputAction.CallbackContext> started)
            {
                _input = input;
                _started = started;
            }

            void IDisposable.Dispose() => _input.started -= _started;
        }
    }
}
