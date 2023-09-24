using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    using Common;
    using Common.Dependencies;
    using Features.MainMenu;
    using Features.SceneOperations;
    using Features.SceneOperations.Loader;

    internal sealed class MainMenuScope : LifetimeScope
    {
        [field: SerializeField] public MainMenuViewModel MainMenuViewModel { get; private set; }
        [field: SerializeField] public AssetReference GameplayScene { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<MainMenuEntryPoint>(Lifetime.Scoped);
            builder.RegisterEntryPointExceptionHandler(Handlers.DefaultExceptionHandler);

            builder.Register(_ =>
            {
                var prefetcher = new SceneLoadingPrefetcher(GameplayScene, PlayerLoopTiming.Initialization, priority: 1);
                var handler = prefetcher.PrefetchAsync(Application.exitCancellationToken);
                var loadGameplay = Operations.CreateLoadingJob(handler);

                return new MainMenuModel(loadGameplay);

            }, Lifetime.Scoped);

            builder.RegisterComponent(MainMenuViewModel);

            builder.AddLogger();
        }
    }
}
