using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace MagicSwords.Features.SceneOperations
{
    using Generic.Functional;
    using Loader;
    using Switcher;

    public delegate UniTask<AsyncResult<AsyncLazy>> LoadingJob(CancellationToken cancellation = default);

    internal static class Operations
    {
        public static LoadingJob CreateLoadingJob
        (
            SceneLoadingPrefetcher.Handler handler
        ) {
            var continuation = handler.Continuation;
            var yieldTarget = handler.YieldContext;

            return token => SceneAsyncRoutines.PrefetchedLoadingJob(continuation, yieldTarget, token);
        }

        public static Func<CancellationToken, UniTask<AsyncResult>> CreateLoadingJob
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget
        ) {
            return token => SceneAsyncRoutines.RegularLoadingJob(target, yieldTarget, token);
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
