using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneLoader.Switcher
{
    using Generic.Functional;

    internal sealed class PrefetchBasedSceneSwitcher : ISceneSwitcher
    {
        private readonly UniTask<SceneInstance> _prefetching;
        private readonly PlayerLoopTiming _yieldTarget;

        public PrefetchBasedSceneSwitcher(UniTask<SceneInstance> prefetching, PlayerLoopTiming yieldTarget)
        {
            _prefetching = prefetching;
            _yieldTarget = yieldTarget;
        }

        async UniTask<AsyncResult> ISceneSwitcher.SwitchAsync(CancellationToken cancellation)
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
