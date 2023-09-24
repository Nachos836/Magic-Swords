using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.Logger;
    using Features.ApplicationEntry;

    internal static class ApplicationEntryDependencies
    {
        public static IContainerBuilder AddApplicationEntry(this IContainerBuilder builder, ILogger logger)
        {
            builder.RegisterEntryPoint<ApplicationEntryPoint>()
                .WithParameter(PlayerLoopTiming.Initialization);
            builder.RegisterEntryPointExceptionHandler(logger.LogException);

            return builder;
        }
    }
}
