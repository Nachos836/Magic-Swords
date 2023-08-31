using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneLoader.Loader
{
    using Generic.Functional;

    internal static class PrefetchBasedSceneLoader
    {
        public static Func<CancellationToken, UniTask<AsyncResult>> CreateLoadingJob(SceneLoadingPrefetcher.Handler handler)
        {
            var continuation = handler.Continuation;
            var yieldTarget = handler.YieldContext;

            return token => LoadingJob(continuation, yieldTarget, token);
        }

        private static async UniTask<AsyncResult> LoadingJob
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
    }
}
