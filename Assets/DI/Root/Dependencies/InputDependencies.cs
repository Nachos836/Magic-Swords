using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root.Dependencies
{
    using Features.Input;

    internal static class InputDependencies
    {
        public static IContainerBuilder AddPlayerInput(this IContainerBuilder builder)
        {
            builder.Register<PlayerInputWrapper>(Lifetime.Singleton)
                .WithParameter(PlayerLoopTiming.Update)
                .As<IAsyncStartable>()
                .AsSelf();

            return builder;
        }
    }
}
