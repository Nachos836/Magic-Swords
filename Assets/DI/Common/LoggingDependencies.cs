using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

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

        public static IContainerBuilder AddScopeEntry<TEntryPoint>
        (
            this IContainerBuilder builder,
            ILogger logger,
            PlayerLoopTiming initializationPoint = PlayerLoopTiming.Initialization
        ) {
            builder.RegisterEntryPoint<TEntryPoint>()
                .WithParameter(initializationPoint);
            builder.RegisterEntryPointExceptionHandler(logger.LogException);

            return builder;
        }
    }
}
