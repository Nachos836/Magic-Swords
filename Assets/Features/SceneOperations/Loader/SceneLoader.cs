﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneOperations.Loader
{
    using Generic.Functional;

    internal static class SceneLoader
    {
        internal static async UniTask<AsyncResult> PrefetchedLoadingJob
        (
            AsyncLazy<SceneInstance> continuation,
            PlayerLoopTiming yieldTarget,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (prefetchingWasCanceled, sceneInstance) = await continuation.Task.SuppressCancellationThrow();
                if (prefetchingWasCanceled) return AsyncResult.Cancel;

                var activatingWasCanceled = await sceneInstance
                    .ActivateAsync()
                    .ToUniTask(timing: yieldTarget, cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                return activatingWasCanceled is not true
                    ? AsyncResult.Success
                    : AsyncResult.Cancel;
            }
            catch (Exception unexpected)
            {
                return unexpected;
            }
        }

        internal static async UniTask<AsyncResult> RegularLoadingJob
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (wasCanceled, _) = await target
                    .LoadSceneAsync(LoadSceneMode.Additive, activateOnLoad: true, priority: 100)
                    .ToUniTask(timing: yieldTarget, cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                return wasCanceled is not true
                    ? AsyncResult.Success
                    : AsyncResult.Cancel;
            }
            catch (Exception unexpected)
            {
                return unexpected;
            }
        }
    }
}
