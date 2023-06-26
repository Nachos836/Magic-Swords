using VContainer;

namespace MagicSwords.DI.ApplicationEntry.Dependencies
{
    using Features.Miscellaneous;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature(this IContainerBuilder builder)
        {
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Scoped);

            return builder;
        }
    }
}