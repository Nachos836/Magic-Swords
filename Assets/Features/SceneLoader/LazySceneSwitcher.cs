using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneLoader
{
    using Generic.Functional;

    public sealed class LazySceneSwitch : IScenePrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        private UniTask<SceneInstance>? _prefetching;

        public LazySceneSwitch(AssetReference target, PlayerLoopTiming yieldTarget)
        {
            _target = target;
            _yieldTarget = yieldTarget;
        }

        void IScenePrefetcher.PrefetchAsync(CancellationToken cancellation)
        {
            var prefetching = _target
                .LoadSceneAsync(activateOnLoad: false)
                .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
                .Preserve();

            prefetching.Forget();

            _prefetching = prefetching;
        }

        public async UniTask<AsyncResult> SwitchAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                if (_prefetching is null) return new Exception("You need to prefetch first!");

                var (prefetchingWasCanceled, sceneInstance) = await _prefetching.Value.SuppressCancellationThrow();
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