using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    using Features.MainMenu;
    using Features.SceneLoader.Loader;

    internal sealed class MainMenuScope : LifetimeScope
    {
        [field: SerializeField] public MainMenuViewModel MainMenuViewModel { get; private set; }
        [field: SerializeField] public AssetReference GameplayScene { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<MainMenuEntry>();
            builder.Register(_ =>
            {
                var prefetcher = new SceneLoadingPrefetcher(GameplayScene, PlayerLoopTiming.Initialization, priority: 1);
                var handler = prefetcher.PrefetchAsync(Application.exitCancellationToken);
                var loadGameplay = PrefetchBasedSceneLoader.CreateLoadingJob(handler);

                return new MainMenuModel(loadGameplay);
            }, Lifetime.Scoped);
            builder.RegisterComponent(MainMenuViewModel);
        }
    }

    internal sealed class MainMenuEntry : IAsyncStartable
    {
        UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Вот наше главное меню!");

            return UniTask.CompletedTask;
        }
    }
}
