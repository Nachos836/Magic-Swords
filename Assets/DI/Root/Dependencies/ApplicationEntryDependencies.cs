using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.ApplicationEntry;

    internal static class ApplicationEntryDependencies
    {
        public static IContainerBuilder AddApplicationEntry(this IContainerBuilder builder, int targetSceneIndex)
        {
            builder
                .RegisterEntryPoint<ApplicationEntryPoint>()
                .WithParameter(targetSceneIndex);

            return builder;
        }
    }
}