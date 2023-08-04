using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace MagicSwords.Features.SceneLoader
{
    using Generic.Functional;

    public sealed class SceneSwitcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        public SceneSwitcher(AssetReference target, PlayerLoopTiming yieldTarget)
        {
            _target = target;
            _yieldTarget = yieldTarget;
        }

        public async UniTask<AsyncResult> SwitchAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (wasCanceled, _) = await _target
                    .LoadSceneAsync()
                    .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
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
