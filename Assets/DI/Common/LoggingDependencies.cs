using VContainer;

namespace MagicSwords.DI.Common
{
    using Features.Logger;
    using Features.Logger.Loggers;

    internal static class LoggingDependencies
    {
        public static IContainerBuilder AddUnityBasedLogger(this IContainerBuilder builder, out ILogger logger)
        {
#       if DEBUG
            var target = new UnityBasedLogger();
            builder.Register(_ => target, Lifetime.Scoped)
                .As<ILogger>();
            logger = target;
#       else
            var target = new VoidLogger();
            builder.Register<VoidLogger>(Lifetime.Scoped)
                .As<ILogger>();
            logger = target;
#       endif
            return builder;
        }
    }
}
