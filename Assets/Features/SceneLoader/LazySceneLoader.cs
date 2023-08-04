using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneLoader
{
    using Generic.Functional;

    public sealed class LazySceneLoader : IScenePrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly int _priority;
        private UniTask<SceneInstance>? _prefetching;

        public LazySceneLoader(AssetReference target, PlayerLoopTiming yieldTarget, int priority)
        {
            _target = target;
            _yieldTarget = yieldTarget;
            _priority = priority;
        }

        void IScenePrefetcher.PrefetchAsync(CancellationToken cancellation)
        {
            var prefetching = _target
                .LoadSceneAsync(loadMode: LoadSceneMode.Additive, activateOnLoad: false, _priority)
                .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
                .Preserve();

            prefetching.Forget();

            _prefetching = prefetching;
        }

        public async UniTask<AsyncResult> LoadAsync(CancellationToken cancellation = default)
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