using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace MagicSwords.Features.SceneOperations
{
    using Generic.Functional;
    using Loader;
    using Switcher;

    internal static class Operations
    {
        public static Func<CancellationToken, UniTask<AsyncResult>> CreateLoadingJob
        (
            SceneLoadingPrefetcher.Handler handler
        ) {
            var continuation = handler.Continuation;
            var yieldTarget = handler.YieldContext;

            return token => SceneLoader.PrefetchedLoadingJob(continuation, yieldTarget, token);
        }

        public static Func<CancellationToken, UniTask<AsyncResult>> CreateLoadingJob
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget
        ) {
            return token => SceneLoader.RegularLoadingJob(target, yieldTarget, token);
        }

        public static Func<CancellationToken, UniTask<AsyncResult>> CreateSwitchingJob(SceneSwitchingPrefetcher.Handler handler)
        {
            var continuation = handler.Continuation;
            var yieldTarget = handler.YieldContext;

            return token => SceneSwitcher.PrefetchedSwitchingJob(continuation, yieldTarget, token);
        }

        public static Func<CancellationToken, UniTask<AsyncResult>> CreateSwitchingJob
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget
        ) {
            return token => SceneSwitcher.RegularSwitchingJob(target, yieldTarget, token);
        }
    }
}
