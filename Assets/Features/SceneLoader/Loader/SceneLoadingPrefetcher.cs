using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneLoader.Loader
{
    internal readonly ref struct SceneLoadingPrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly int _priority;

        public SceneLoadingPrefetcher(AssetReference target, PlayerLoopTiming yieldTarget, int priority)
        {
            _target = target;
            _yieldTarget = yieldTarget;
            _priority = priority;
        }

        public (UniTask<SceneInstance> Handler, PlayerLoopTiming YieldTarget) PrefetchAsync(CancellationToken cancellation = default)
        {
            var prefetching = _target
                .LoadSceneAsync(loadMode: LoadSceneMode.Additive, activateOnLoad: false, _priority)
                .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
                .Preserve();

            prefetching.Forget();

            return (prefetching, _yieldTarget);
        }
    }
}
