﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;

using static System.Threading.CancellationTokenSource;

namespace MagicSwords.Features.Input.Actions.PlayerDriven
{
    internal interface IUIActionsProvider
    {
        ref Autogenerated_PlayerInputActions.UIActions Get();
    }

    internal interface IReadingActionsProvider
    {
        ref Autogenerated_PlayerInputActions.ReadingActions Get();
    }

    internal sealed class PlayerInputWrapper : Autogenerated_PlayerInputActions, IAsyncStartable, IDisposable, IUIActionsProvider, IReadingActionsProvider
    {
        private readonly PlayerLoopTiming _initializationPoint;

        private (bool Fetched, UIActions Value) _uiActions;
        private (bool Fetched, ReadingActions Value) _readingActions;
        private CancellationTokenSource _produceInputUpdates = default!;

        public PlayerInputWrapper(PlayerLoopTiming initializationPoint)
        {
            _initializationPoint = initializationPoint;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _produceInputUpdates = CreateLinkedTokenSource(cancellation);

            Enable();

            AcquireInputJobAsync(_initializationPoint, _produceInputUpdates.Token)
                .Forget();
        }

        void IDisposable.Dispose()
        {
            _produceInputUpdates.Cancel();

            if (_uiActions.Fetched) _uiActions.Value.Disable();
            if (_readingActions.Fetched) _readingActions.Value.Disable();
            Disable();

#       if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(asset);
#       else
            Dispose();
#       endif

            _produceInputUpdates.Dispose();
        }

        ref UIActions IUIActionsProvider.Get()
        {
            if (_uiActions.Fetched) return ref _uiActions.Value;

            _uiActions = (Fetched: true, UI);

            return ref _uiActions.Value;
        }

        ref ReadingActions IReadingActionsProvider.Get()
        {
            if (_readingActions.Fetched) return ref _readingActions.Value;

            _readingActions = (Fetched: true, Reading);

            return ref _readingActions.Value;
        }

        private static async UniTaskVoid AcquireInputJobAsync(PlayerLoopTiming timing, CancellationToken cancellation = default)
        {
            await UniTask.WaitWhile(static () =>
            {
                InputSystem.Update();

                return true;

            }, timing, cancellation)
                .SuppressCancellationThrow();
        }
    }
}
