using UnityEngine.AddressableAssets;
using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.MainMenu;
    using Features.ApplicationExit;

    using static Common.SceneLoaderDependencies;

    internal static class MainMenuModelDependencies
    {
        public static IContainerBuilder AddMainMenuModel(this IContainerBuilder builder, AssetReference gameplayScene)
        {
            builder.Register(resolver =>
            {
                var loadGameplay = AddSceneLoadingJob(gameplayScene, loadInstantly: false);
                var exitRoutine = resolver.Resolve<IApplicationExitRoutine>();

                return new MainMenuModel(loadGameplay, exitRoutine);

            }, Lifetime.Scoped)
                .AsSelf();

            return builder;
        }
    }
}
