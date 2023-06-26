using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace MagicSwords.Features.Input.PlayerDriven
{
    internal sealed class PlayerInputActions : IAsyncStartable, IMovementInputActivation, IMovementInputSubscription, IMovementInputSubscriptionAsync, IDisposable
    {
        private readonly Autogenerated_PlayerInputActions _playerInputActions;
        private readonly InputAction _playerMovement;
        private Autogenerated_PlayerInputActions.PlayerActions _playerActions;

        public PlayerInputActions()
        {
            _playerInputActions = new ();
            _playerActions = _playerInputActions.Player;
            _playerMovement = _playerActions.Movement;
        }

        UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _playerInputActions.Enable();
            _playerActions.Enable();
            _playerMovement.Enable();

            InputFetchLoopAsync(cancellation).Forget();

            return UniTask.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            _playerMovement.Disable();
            _playerActions.Disable();
            _playerInputActions.Disable();
            
            _playerMovement.Dispose();
            _playerInputActions.Dispose();
        }

        void IMovementInputActivation.Enable() => _playerMovement.Enable();
        void IMovementInputActivation.Disable() => _playerMovement.Disable();

        IDisposable IMovementInputSubscription.Subscribe<TState>(Action<InputAction.CallbackContext> callback)
        {
            return new TState().Subscribe(_playerMovement, callback);
        }

        IUniTaskAsyncDisposable IMovementInputSubscriptionAsync.SubscribeAsync<TState>(Func<InputAction.CallbackContext, CancellationToken, UniTaskVoid> callback, CancellationToken cancellation)
        {
            return new TState().SubscribeAsync(_playerMovement, callback, cancellation);
        }

        private static async UniTaskVoid InputFetchLoopAsync(CancellationToken cancellation = default)
        {
            while (cancellation.IsCancellationRequested is false)
            {
                InputSystem.Update();

                await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellation)
                    .SuppressCancellationThrow();
            }
        }
    }

    internal interface IMovementInputActivation
    {
        void Enable();
        void Disable();
    }

    internal interface IMovementInputSubscription
    {
        IDisposable Subscribe<TState>(Action<InputAction.CallbackContext> callback) where TState : struct, IActionState;
    }

    internal interface IMovementInputSubscriptionAsync
    {

        IUniTaskAsyncDisposable SubscribeAsync<TState>(Func<InputAction.CallbackContext, CancellationToken, UniTaskVoid> callback, CancellationToken cancellation = default) where TState : struct, IActionStateAsync;
    }

    internal interface IActionState
    {
        IDisposable Subscribe(InputAction inputs, Action<InputAction.CallbackContext> callback);
    }

    internal interface IActionStateAsync
    {
        IUniTaskAsyncDisposable SubscribeAsync(InputAction inputs, Func<InputAction.CallbackContext, CancellationToken, UniTaskVoid> callback, CancellationToken cancellation = default);
    }
}
