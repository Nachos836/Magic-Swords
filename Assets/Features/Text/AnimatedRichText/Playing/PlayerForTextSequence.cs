using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Generic.Functional;
    using Generic.Sequencer;
    using Logger;

    internal sealed class PlayerForTextSequence : ITextPlayer
    {
        private readonly ILogger _logger;
        private readonly Sequencer _sequencer;

        public PlayerForTextSequence(ILogger logger, Sequencer sequencer)
        {
            _logger = logger;
            _sequencer = sequencer;
        }

        async UniTask<AsyncResult> ITextPlayer.PlayAsync(CancellationToken cancellation)
        {
            _logger.LogInformation("Вот начало секвенции диалога!");

            var outcome = await _sequencer.StartAsync(cancellation);
            outcome.Match
            (
                success: _ => _logger.LogInformation("Успешно завершено!"),
                cancellation: _ => _logger.LogWarning("Выполнение было отменено!"),
                error: (exception, _) => _logger.LogException(exception),
                cancellation
            );

            return outcome;
        }
    }
}
