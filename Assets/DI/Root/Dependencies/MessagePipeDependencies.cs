using MessagePipe;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    internal static class MessagePipeDependencies
    {
        public static IContainerBuilder AddMessagePipeFeature(this IContainerBuilder builder)
        {
            builder.RegisterMessagePipe(configure: static pipeOptions =>
            {
#               if DEBUG
                // EnableCaptureStackTrace slows performance
                // Recommended to use only in DEBUG and in profiling, disable it.
                pipeOptions.EnableCaptureStackTrace = true;
                pipeOptions.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Throw;
#               endif

                pipeOptions.InstanceLifetime = InstanceLifetime.Scoped;
                pipeOptions.RequestHandlerLifetime = InstanceLifetime.Scoped;
            });

            builder.RegisterBuildCallback(static resolver => GlobalMessagePipe.SetProvider(resolver.AsServiceProvider()));

            return builder;
        }
    }
}
