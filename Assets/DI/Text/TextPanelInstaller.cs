using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Common;
    using Features.Text;

    internal sealed class TextPanelInstaller : IInstaller
    {
        private readonly MessagePipeOptions _messagePipeOptions;

        public TextPanelInstaller(MessagePipeOptions messagePipeOptions)
        {
            _messagePipeOptions = messagePipeOptions;
        }

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder
                .RegisterMessageBroker<IPresentJob>(_messagePipeOptions)
                .AddUnityBasedTimeProvider();
        }
    }
}
