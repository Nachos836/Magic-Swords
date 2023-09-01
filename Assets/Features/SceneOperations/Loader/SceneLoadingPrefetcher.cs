using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneOperations.Loader
{
    internal readonly ref struct SceneLoadingPrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly int _priority;
        private readonly bool _instantLoad;

        public SceneLoadingPrefetcher
        (
            AssetReference target,
            PlayerLoopTiming yieldTarget,
            int priority,
            bool instantLoad = false
        ) {
            _target = target;
            _yieldTarget = yieldTarget;
            _priority = priority;
            _instantLoad = instantLoad;
        }

        public Handler PrefetchAsync(CancellationToken cancellation = default)
        {
            var target = _target;
            var priority = _priority;
            var yieldTarget = _yieldTarget;

            var prefetching = _instantLoad is false
                ? LoadWithWorkaroundDelayAsync(yieldTarget, target, priority, cancellation)
                    .ToAsyncLazy()
                : LoadAsync(yieldTarget, target, priority, cancellation)
                    .ToAsyncLazy();

            return new Handler(prefetching, _yieldTarget);

            static async UniTask<SceneInstance> LoadAsync(PlayerLoopTiming yieldTarget, AssetReference target, int priority, CancellationToken token = default)
            {
                return await target
                    .LoadSceneAsync(loadMode: LoadSceneMode.Additive, activateOnLoad: true, priority)
                    .ToUniTask(progress: null, timing: yieldTarget, cancellationToken: token);
            }

            /*
             * This workaround could be removed if versions of packages will be
             * "com.unity.addressables": "1.8.5"
             * "com.unity.scriptablebuildpipeline": "1.7.3"
             * Thus compatibility and builds reliability are not guaranteed
             */
            [Obsolete("Workaround for https://issuetracker.unity3d.com/issues/loadsceneasync-allowsceneactivation-flag-is-ignored-in-awake")]
            static async UniTask<SceneInstance> LoadWithWorkaroundDelayAsync(PlayerLoopTiming yieldTarget, AssetReference target, int priority, CancellationToken token = default)
            {
                await UniTask.Yield(yieldTarget, token);

                return await target
                    .LoadSceneAsync(loadMode: LoadSceneMode.Additive, activateOnLoad: false, priority)
                    .ToUniTask(progress: null, timing: yieldTarget, cancellationToken: token);
            }
        }

        public readonly ref struct Handler
        {
            public readonly AsyncLazy<SceneInstance> Continuation;
            public readonly PlayerLoopTiming YieldContext;

            public Handler(AsyncLazy<SceneInstance> continuation, PlayerLoopTiming yieldContext)
            {
                Continuation = continuation;
                YieldContext = yieldContext;
            }
        }
    }
}
