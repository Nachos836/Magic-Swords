using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Common
{
    internal static class ScopeEntryDependencies
    {
        public static IContainerBuilder AddScopeEntry<TEntryPoint>
        (
            this IContainerBuilder builder,
            PlayerLoopTiming initializationPoint = PlayerLoopTiming.Initialization
        ) {
            builder.RegisterEntryPoint<TEntryPoint>()
                .WithParameter(initializationPoint);

            return builder;
        }
    }
}
