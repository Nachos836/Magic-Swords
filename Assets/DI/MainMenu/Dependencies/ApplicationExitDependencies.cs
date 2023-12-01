using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.ApplicationExit;

    internal static class ApplicationExitDependencies
    {
        public static IContainerBuilder AddApplicationExitRoutine(this IContainerBuilder builder)
        {
            builder.Register<IApplicationExitRoutine, ApplicationExitRoutine>(Lifetime.Scoped);

            return builder;
        }
    }
}
