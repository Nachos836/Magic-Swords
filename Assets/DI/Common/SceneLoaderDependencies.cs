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
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            builder.Register
            (
                implementationConfiguration: _ => AddAsyncSceneLoadingJob(target, loadInstantly, cancellation),
                Lifetime.Scoped
            );

            return builder;
        }

        public static IContainerBuilder AddSceneLoaderFeature
        (
            this IContainerBuilder builder,
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            builder.Register
            (
                implementationConfiguration: _ => AddAsyncSceneLoadingJob(target, arguments, loadInstantly, cancellation),
                Lifetime.Scoped
            );

            return builder;
        }

        public static IContainerBuilder AddSceneLoaderFeature
        (
            this IContainerBuilder builder,
            LifetimeScope parent,
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            builder.Register
            (
                implementationConfiguration: _ => AddAsyncSceneLoadingJob(parent, target, arguments, loadInstantly, cancellation),
                Lifetime.Scoped
            );

            return builder;
        }

        public static LoadingJob AddAsyncSceneLoadingJob
        (
            AssetReference target,
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync(cancellation);

            return Operations.CreateLoadingJob(handler);
        }

        public static LoadingJob AddAsyncSceneLoadingJob
        (
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync
            (
                passExtraDependencies: () => LifetimeScope.Enqueue(arguments),
                cancellation
            );

            return Operations.CreateLoadingJob(handler);
        }

        public static LoadingJob AddAsyncSceneLoadingJob
        (
            LifetimeScope parent,
            AssetReference target,
            IInstaller arguments,
            bool loadInstantly,
            CancellationToken cancellation = default
        ) {
            var prefetcher = new SceneLoadingPrefetcher
            (
                target,
                yieldPoint: Initialization,
                priority: 100,
                instantLoad: loadInstantly
            );

            var handler = prefetcher.PrefetchAsync
            (
                passExtraDependencies: () => new ScopeOperations
                (
                    LifetimeScope.EnqueueParent(parent),
                    LifetimeScope.Enqueue(arguments)
                ),
                cancellation
            );

            return Operations.CreateLoadingJob(handler);
        }

        private sealed class ScopeOperations : IDisposable
        {
            private readonly LifetimeScope.ParentOverrideScope _parentOverrideScope;
            private readonly LifetimeScope.ExtraInstallationScope _extraInstallation;

            public ScopeOperations
            (
                LifetimeScope.ParentOverrideScope parentOverrideScope,
                LifetimeScope.ExtraInstallationScope extraInstallation
            ) {
                _parentOverrideScope = parentOverrideScope;
                _extraInstallation = extraInstallation;
            }

            void IDisposable.Dispose()
            {
                using (_extraInstallation)
                {
                    _parentOverrideScope.Dispose();
                }
            }
        }
    }
}
