using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.Text.Players.SequencePlayer
{
    using Logger;
    using Generic.Sequencer;

    internal sealed class SequencePlayerEntryPoint : IAsyncStartable
    {
        private readonly ILogger _logger;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly Sequencer _sequencer;

        public SequencePlayerEntryPoint
        (
            ILogger logger,
            PlayerLoopTiming initializationPoint,
            Sequencer sequencer
        ) {
            _logger = logger;
            _initializationPoint = initializationPoint;
            _sequencer = sequencer;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Вот начало диалога!");

            var outcome = await _sequencer.StartAsync(cancellation);
            outcome.Match
            (
                success: _ => _logger.LogInformation("Успешно завершено!"),
                cancellation: _ => _logger.LogWarning("Выполнение было отменено!"),
                error: (exception, _) => _logger.LogException(exception),
                cancellation
            );
        }
    }
}
