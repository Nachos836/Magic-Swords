using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    using Common;
    using Dependencies;
    using Features.MainMenu;
    using Root.Dependencies;

    internal sealed class MainMenuScope : LifetimeScope
    {
        [SerializeField] private MainMenuViewModel _mainMenuViewModel;
        [SerializeField] private AssetReference _gameplayScene;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddApplicationExitRoutine()
                .AddMainMenuModel(_gameplayScene)
                .AddMainMenuViewModel(_mainMenuViewModel)
                .AddPlayerInput()
                .AddUIInput();
        }
    }
}
