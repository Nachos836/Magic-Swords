using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Common;
    using Features.Text;

    internal sealed class TextPanelScope : LifetimeScope { }

    internal sealed class TextPanelInstaller : IInstaller
    {
        private readonly MessagePipeOptions _messagePipeOptions;
        private readonly IText _text;

        public TextPanelInstaller(MessagePipeOptions messagePipeOptions, IText text)
        {
            _messagePipeOptions = messagePipeOptions;
            _text = text;
        }

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder
                .RegisterMessageBroker<IPresentJob>(_messagePipeOptions)
                .AddUnityBasedLogger(out var logger)
                .AddScopeEntry<TextPresentationEntryPoint>(logger)
                .AddUnityBasedTimeProvider()
                .RegisterInstance(_text);
        }
    }
}
