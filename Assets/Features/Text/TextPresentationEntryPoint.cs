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
        private readonly Player _player;
        private readonly ILogger _logger;

        public TextPresentationEntryPoint(PlayerLoopTiming initiatingPoint, Player player, ILogger logger)
        {
            _initiatingPoint = initiatingPoint;
            _player = player;
            _logger = logger;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await UniTask.Yield(_initiatingPoint, cancellation);

            var result = await _player.PlayAsync(cancellation);
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
