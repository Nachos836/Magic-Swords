using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.ApplicationEntry;

    internal static class ApplicationEntryDependencies
    {
        public static IContainerBuilder AddApplicationEntry(this IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ApplicationEntryPoint>();

            return builder;
        }
    }
}
