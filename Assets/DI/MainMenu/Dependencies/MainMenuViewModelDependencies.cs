using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.MainMenu;

    internal static class MainMenuViewModelDependencies
    {
        public static IContainerBuilder AddMainMenuViewModel(this IContainerBuilder builder, MainMenuViewModel mainMenuViewModel)
        {
            builder.RegisterBuildCallback(container =>
            {
                container.Inject(mainMenuViewModel);
            });

            return builder;
        }
    }
}
