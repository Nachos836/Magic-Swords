using VContainer;

namespace MagicSwords.DI.Common
{
    using Features.TimeProvider;
    using Features.TimeProvider.Providers;

    internal static class TimeProvidingDependencies
    {
        public static IContainerBuilder AddUnityBasedTimeProvider(this IContainerBuilder builder, Lifetime lifetime = Lifetime.Scoped)
        {
            builder.Register<UnityTimeProvider>(lifetime)
                .As<ICurrentTimeProvider, IDeltaTimeProvider, IFixedCurrentTimeProvider, IFixedDeltaTimeProvider>();

            return builder;
        }
    }
}
