using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneOperations.Switcher
{
    using Generic.Functional;

    internal static class SceneSwitcher
    {
        internal static async UniTask<AsyncResult> PrefetchedSwitchingJob
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

                return activatingWasCanceled
                    ? AsyncResult.Cancel
                    : AsyncResult.Success;
            }
            catch (Exception unexpected)
            {
                return unexpected;
            }
        }

        internal static async UniTask<AsyncResult> RegularSwitchingJob
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (wasCanceled, _) = await target
                    .LoadSceneAsync()
                    .ToUniTask(timing: yieldTarget, cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                return wasCanceled
                    ? AsyncResult.Cancel
                    : AsyncResult.Success;
            }
            catch (Exception unexpected)
            {
                return unexpected;
            }
        }
    }
}
