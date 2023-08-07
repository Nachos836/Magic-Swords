using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneLoader.Loader
{
    using Generic.Functional;

    internal sealed class PrefetchBasedSceneLoader : ISceneLoader
    {
        private readonly UniTask<SceneInstance> _prefetching;
        private readonly PlayerLoopTiming _yieldTarget;

        public PrefetchBasedSceneLoader(UniTask<SceneInstance> prefetching, PlayerLoopTiming yieldTarget)
        {
            _prefetching = prefetching;
            _yieldTarget = yieldTarget;
        }

        async UniTask<AsyncResult> ISceneLoader.LoadAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (prefetchingWasCanceled, sceneInstance) = await _prefetching.SuppressCancellationThrow();
                if (prefetchingWasCanceled) return AsyncResult.Cancel;

                var activatingWasCanceled = await sceneInstance
                    .ActivateAsync()
                    .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
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
