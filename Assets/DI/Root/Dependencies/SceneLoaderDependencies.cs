using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.SceneLoader;

    internal static class SceneLoaderDependencies
    {
        public static IContainerBuilder AddSceneLoaderFeature(this IContainerBuilder builder)
        {
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Scoped);

            return builder;
        }
    }
}