using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.ApplicationEntry.Dependencies
{
    internal static class ApplicationEntryDependencies
    {
        public static IContainerBuilder AddApplicationEntry<TEntry>(this IContainerBuilder builder, int targetSceneIndex)
        {
            builder.RegisterEntryPoint<TEntry>().WithParameter(targetSceneIndex);

            return builder;
        }
    }
}