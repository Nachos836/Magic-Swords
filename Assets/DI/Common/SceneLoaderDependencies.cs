using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.DI.Common
{
    using Features.SceneOperations;
    using Features.SceneOperations.Loader;

    using LoadingJob = Func<CancellationToken, UniTask<Features.Generic.Functional.AsyncResult>>;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature
        (
            this IContainerBuilder builder,
            AssetReference target,
            bool loadInstantly
        ) {
            builder.Register(_ => AddSceneLoadingJob(target, loadInstantly), Lifetime.Scoped);

            return builder;
        }

        public static IContainerBuilder AddSceneLoaderFeature
        (
            this IContainerBuilder builder,
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly
        ) {
            builder.Register(_ => AddSceneLoadingJob(target, arguments, loadInstantly), Lifetime.Scoped);

            return builder;
        }

        public static IContainerBuilder AddSceneLoaderFeature
        (
            this IContainerBuilder builder,
            LifetimeScope parent,
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly
        ) {
            builder.Register(_ => AddSceneLoadingJob(parent, target, arguments, loadInstantly), Lifetime.Scoped);

            return builder;
        }

        public static LoadingJob AddSceneLoadingJob(AssetReference target, bool loadInstantly)
        {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync(Application.exitCancellationToken);

            return Operations.CreateLoadingJob(handler);
        }

        public static LoadingJob AddSceneLoadingJob(AssetReference target, IInstaller arguments, bool loadInstantly)
        {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync
            (
                pathExtraDependencies: () => LifetimeScope.Enqueue(arguments),
                Application.exitCancellationToken
            );

            return Operations.CreateLoadingJob(handler);
        }

        public static LoadingJob AddSceneLoadingJob(LifetimeScope parent, AssetReference target, IInstaller arguments, bool loadInstantly)
        {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync
            (
                pathExtraDependencies: () =>
                {
                    var scopeOperations = DisposableBag.Create
                    (
                        LifetimeScope.EnqueueParent(parent),
                        LifetimeScope.Enqueue(arguments)
                    );

                    return scopeOperations;
                },
                Application.exitCancellationToken
            );

            return Operations.CreateLoadingJob(handler);
        }
    }
}
