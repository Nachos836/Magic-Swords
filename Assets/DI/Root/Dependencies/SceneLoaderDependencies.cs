using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.SceneLoader;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature(this IContainerBuilder builder, AssetReference target)
        {
            builder.Register(resolver =>
            {
                var loader = new LazySceneLoader(target, PlayerLoopTiming.Initialization, priority: 100);

                ((IScenePrefetcher) loader).PrefetchAsync();

                return loader;
            }, Lifetime.Scoped);

            return builder;
        }
    }
}