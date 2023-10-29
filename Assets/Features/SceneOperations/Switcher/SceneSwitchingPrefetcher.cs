using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.SceneOperations.Switcher
{
    internal readonly ref struct SceneSwitchingPrefetcher
    {
        private readonly AssetReference _target;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly int _priority;
        private readonly bool _instantLoad;

        public SceneSwitchingPrefetcher
        (
            AssetReference target,
            PlayerLoopTiming yieldPoint,
            int priority,
            bool instantLoad = false
        ) {
            _target = target;
            _yieldPoint = yieldPoint;
            _priority = priority;
            _instantLoad = instantLoad;
        }

        public Handler PrefetchAsync(CancellationToken cancellation = default)
        {
            var target = _target;
            var priority = _priority;
            var yieldPoint = _yieldPoint;

            var prefetching = _instantLoad is false
                ? LoadWithWorkaroundDelayAsync(target, yieldPoint, priority, cancellation)
                    .ToAsyncLazy()
                : LoadAsync(target, yieldPoint, priority, cancellation)
                    .ToAsyncLazy();

            return new Handler(prefetching, _yieldPoint);

            static async UniTask<SceneInstance> LoadAsync
            (
                AssetReference target,
                PlayerLoopTiming yieldPoint,
                int priority,
                CancellationToken token = default
            ) {
                return await target
                    .LoadSceneAsync(loadMode: LoadSceneMode.Single, activateOnLoad: true, priority)
                    .ToUniTask(progress: null, timing: yieldPoint, cancellationToken: token);
            }

            static async UniTask<SceneInstance> LoadWithWorkaroundDelayAsync
            (
                AssetReference target,
                PlayerLoopTiming yieldPoint,
                int priority,
                CancellationToken token = default
            ) {
                await UniTask.Yield(yieldPoint, token);

                return await target
                    .LoadSceneAsync(loadMode: LoadSceneMode.Single, activateOnLoad: false, priority)
                    .ToUniTask(progress: null, timing: yieldPoint, cancellationToken: token);
            }
        }

        internal readonly ref struct Handler
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
