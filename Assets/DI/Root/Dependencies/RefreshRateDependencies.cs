using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.RefreshRateSetup;

    internal static class RefreshRateDependencies
    {
        public static IContainerBuilder AddRefreshRateSetupFeature(this IContainerBuilder builder, float framesPerSecond)
        {
            builder.Register<FixedUpdateRateSetter>(Lifetime.Singleton)
                .WithParameter(framesPerSecond)
                .WithParameter(PlayerLoopTiming.Initialization)
                .As<IAsyncStartable>();

            return builder;
        }
    }
}
