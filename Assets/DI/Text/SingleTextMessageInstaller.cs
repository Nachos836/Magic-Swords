using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Common;
    using Dependencies;
    using Features.Text;
    using Features.Text.AnimatedRichText.Playing;

    internal sealed class SingleTextMessageInstaller : IInstaller
    {
        private readonly IText _text;
        private readonly PlayerLoopTiming _yieldPoint;

        public SingleTextMessageInstaller(IText text, PlayerLoopTiming yieldPoint)
        {
            _text = text;
            _yieldPoint = yieldPoint;
        }

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder
                .AddScopeEntry<TextPresentationEntryPoint>()
                .AddUnityBasedTimeProvider()
                .Register<PlayerForSingleText>(Lifetime.Scoped)
                    .WithParameter(_yieldPoint)
                    .WithParameter(_text)
                    .As<ITextPlayer>();
        }
    }

    internal sealed class SequencedTextMessageInstaller : IInstaller
    {
        private readonly IText[] _message;

        public SequencedTextMessageInstaller(IText[] message) => _message = message;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder
                .AddScopeEntry<TextPresentationEntryPoint>()
                .AddUnityBasedTimeProvider()
                .AddSequencedTextPresenter(_message)
                .Register<PlayerForTextSequence>(Lifetime.Scoped)
                    .As<ITextPlayer>();
        }
    }
}
