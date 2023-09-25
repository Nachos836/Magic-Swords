using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.MainMenu;
    using Features.SceneOperations;
    using Features.SceneOperations.Loader;

    internal static class MainMenuModelDependencies
    {
        public static IContainerBuilder AddMainMenuModel(this IContainerBuilder builder, AssetReference gameplayScene)
        {
            var prefetcher = new SceneLoadingPrefetcher(gameplayScene, PlayerLoopTiming.Initialization, priority: 1);
            var handler = prefetcher.PrefetchAsync(Application.exitCancellationToken);
            var loadGameplay = Operations.CreateLoadingJob(handler);

            builder.Register<MainMenuModel>(Lifetime.Scoped)
                .AsSelf()
                .WithParameter(loadGameplay);

            return builder;
        }
    }
}
