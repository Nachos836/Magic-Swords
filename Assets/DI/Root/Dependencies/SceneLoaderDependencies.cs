using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.SceneLoader.Loader;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature(this IContainerBuilder builder, AssetReference target)
        {
            builder.Register(_ =>
            {
                var prefetcher = new SceneLoadingPrefetcher
                (
                    target,
                    yieldTarget: Initialization,
                    priority: 100,
                    instantLoad: true
                );

                var handler = prefetcher.PrefetchAsync(Application.exitCancellationToken);

                return PrefetchBasedSceneLoader.CreateLoadingJob(handler);

            }, Lifetime.Scoped);

            return builder;
        }
    }
}