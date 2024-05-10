using MessagePipe;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    internal static class MessagePipeDependencies
    {
        public static IContainerBuilder AddMessagePipeFeature(this IContainerBuilder builder, out MessagePipeOptions options)
        {
            options = builder.RegisterMessagePipe(configure: static pipeOptions =>
            {
#               if DEBUG
                // EnableCaptureStackTrace slows performance
                // Recommended to use only in DEBUG and in profiling, disable it.
                pipeOptions.EnableCaptureStackTrace = true;
                pipeOptions.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Throw;
#               endif

                pipeOptions.InstanceLifetime = InstanceLifetime.Singleton;
                pipeOptions.RequestHandlerLifetime = InstanceLifetime.Singleton;
            });

            builder.RegisterBuildCallback(static resolver => GlobalMessagePipe.SetProvider(resolver.AsServiceProvider()));

            return builder;
        }
    }
}
