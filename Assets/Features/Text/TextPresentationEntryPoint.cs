using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.Text
{
    using Logger;
    using AnimatedRichText.Playing;

    internal sealed class TextPresentationEntryPoint : IAsyncStartable
    {
        private readonly PlayerLoopTiming _initiatingPoint;
        private readonly ITextPlayer _textPlayer;
        private readonly ILogger _logger;

        public TextPresentationEntryPoint(PlayerLoopTiming initiatingPoint, ITextPlayer textPlayer, ILogger logger)
        {
            _initiatingPoint = initiatingPoint;
            _textPlayer = textPlayer;
            _logger = logger;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await UniTask.Yield(_initiatingPoint, cancellation);

            var result = await _textPlayer.PlayAsync(cancellation);
            result.Match
            (
                success: _ => _logger.LogInformation("Мы закончили с выводом текста!"),
                cancellation: _ => _logger.LogInformation("Мы отменили вывод текста!"),
                error: (exception, _) => _logger.LogException(exception),
                cancellation
            );
        }
    }
}
