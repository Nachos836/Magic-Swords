using VContainer;

namespace MagicSwords.DI.MainMenu.Dependencies
{
    using Features.ApplicationExit;
    using Features.ApplicationExit.Routines;

    internal static class ApplicationExitDependencies
    {
        public static IContainerBuilder AddApplicationExitRoutine(this IContainerBuilder builder)
        {
            builder.Register<IApplicationExitRoutine>(_ =>
            {
#           if UNITY_EDITOR
                return new RuntimeExitRoutine();
#           else
                return new PlaymodeExitRoutine();
#           endif

            }, Lifetime.Scoped);

            return builder;
        }
    }
}
