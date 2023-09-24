using MagicSwords.Features.Logger;
using MagicSwords.Features.Logger.Loggers;
using VContainer;

namespace MagicSwords.DI.Root.Dependencies
{
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