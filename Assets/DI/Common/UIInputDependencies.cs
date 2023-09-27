using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Common
{
    using Features.Input;
    using Features.Input.Actions;

    internal static class UIInputDependencies
    {
        public static IContainerBuilder AddUIInput(this IContainerBuilder builder)
        {
            builder.Register<UI>(Lifetime.Scoped)
                .WithParameter(PlayerLoopTiming.Update)
                .As<IAsyncStartable>()
                .As<IInputFor<UISubmission>>();

            return builder;
        }
    }
}
