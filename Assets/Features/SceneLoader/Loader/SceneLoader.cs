using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneLoader.Loader
{
    using Generic.Functional;

    internal sealed class SceneLoader : ISceneLoader
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;

        public SceneLoader(AssetReference target, PlayerLoopTiming yieldTarget)
        {
            _target = target;
            _yieldTarget = yieldTarget;
        }

        public async UniTask<AsyncResult> LoadAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return cancellation;

            try
            {
                var (wasCanceled, _) = await _target
                    .LoadSceneAsync(LoadSceneMode.Additive, activateOnLoad: true, priority: 100)
                    .ToUniTask(timing: _yieldTarget, cancellationToken: cancellation)
                    .SuppressCancellationThrow();

                return wasCanceled
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
