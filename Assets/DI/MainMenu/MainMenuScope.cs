using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    using Common;
    using Dependencies;
    using Features.MainMenu;

    internal sealed class MainMenuScope : LifetimeScope
    {
        [field: SerializeField] public MainMenuViewModel MainMenuViewModel { get; private set; }
        [field: SerializeField] public AssetReference GameplayScene { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddLogger(out var logger)
                .AddScopeEntry<MainMenuEntryPoint>(logger)
                .AddApplicationExitRoutine()
                .AddMainMenuModel(GameplayScene)
                .AddMainMenuViewModel(MainMenuViewModel);
        }
    }
}
