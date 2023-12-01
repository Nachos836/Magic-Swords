using System.Threading;
using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.MainMenu;

    using static Common.SceneLoaderDependencies;

    internal static class MainMenuModelDependencies
    {
        public static IContainerBuilder AddMainMenuModel
        (
            this IContainerBuilder builder,
            AssetReference gameplayScene,
            CancellationToken cancellation = default
        ) {
            builder.Register<MainMenuModel>(Lifetime.Scoped)
                .WithParameter(AddAsyncSceneLoadingJob(gameplayScene, loadInstantly: false, cancellation))
                .AsSelf();

            return builder;
        }
    }
}
