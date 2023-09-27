using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Common
{
    using Features.Input;

    internal static class ReadingInputDependencies
    {
        public static IContainerBuilder AddReadingInput(this IContainerBuilder builder)
        {
            builder.Register<Reading>(Lifetime.Scoped)
                .WithParameter(PlayerLoopTiming.Update)
                .As<IAsyncStartable>()
                .As<IInputFor<ReadingSkip>>();

            return builder;
        }
    }
}
