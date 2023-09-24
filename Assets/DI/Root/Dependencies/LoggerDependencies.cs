using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.Logger;
    using Features.Logger.Loggers;

    internal static class LoggerDependencies
    {
        public static IContainerBuilder AddLogger(this IContainerBuilder builder)
        {
#       if DEBUG
            builder.Register<UnityBasedLogger>(Lifetime.Scoped).As<ILogger>();
#       else
            builder.Register<VoidLogger>(Lifetime.Scoped).As<ILogger>();
#       endif
             return builder;
         }
    }
}
