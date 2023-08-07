using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.SceneLoader;
    using Features.SceneLoader.Loader;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature(this IContainerBuilder builder, AssetReference target)
        {
            builder.Register(_ =>
            {
                var prefetcher = new SceneLoadingPrefetcher(target, PlayerLoopTiming.Initialization, priority: 100);
                var prefetching = prefetcher.PrefetchAsync(Application.exitCancellationToken);

                return new PrefetchBasedSceneLoader(prefetching.Handler, prefetching.YieldTarget);

            }, Lifetime.Scoped).As<ISceneLoader>();

            return builder;
        }
    }
}