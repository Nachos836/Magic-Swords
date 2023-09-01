using MessagePipe;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    public static class MessagePipeDependencies
    {
        public static IContainerBuilder AddMessagePipeFeature(this IContainerBuilder builder, out MessagePipeOptions options)
        {
            options = builder.RegisterMessagePipe(configure: pipeOptions =>
            {
#               if DEBUG
                // EnableCaptureStackTrace slows performance
                // Recommended to use only in DEBUG and in profiling, disable it.
                pipeOptions.EnableCaptureStackTrace = true;
#               endif
            });

            builder.RegisterBuildCallback(resolver => GlobalMessagePipe.SetProvider(resolver.AsServiceProvider()));

            return builder;
        }
    }
}
