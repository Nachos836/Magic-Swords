using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    using Common;
    using Features.Text;
    using Features.Text.AnimatedRichText.Playing;

    internal sealed class TextPanelInstaller : IInstaller
    {
        private readonly IText _text;
        private readonly PlayerLoopTiming _yieldPoint;

        public TextPanelInstaller(IText text, PlayerLoopTiming yieldPoint)
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
}
