using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MagicSwords.Features.SceneLoader.Switcher
{
    internal readonly ref struct SceneSwitchingPrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly int _priority;

        public SceneSwitchingPrefetcher(AssetReference target, PlayerLoopTiming yieldTarget, int priority)
        {
            _target = target;
            _yieldTarget = yieldTarget;
            _priority = priority;
        }

        public (UniTask<SceneInstance> Handler, PlayerLoopTiming YieldTarget) PrefetchAsync(CancellationToken cancellation = default)
        {
            var prefetching = _target
                .LoadSceneAsync(activateOnLoad: false, priority: _priority)
                .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
                .Preserve();

            return (prefetching, _yieldTarget);
        }
    }
}
