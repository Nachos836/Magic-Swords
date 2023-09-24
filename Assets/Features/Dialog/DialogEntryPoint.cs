using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.Dialog
{
    using Logger;
    using Generic.Sequencer;

    internal sealed class DialogEntryPoint : IAsyncStartable
    {
        private readonly ILogger _logger;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly Sequencer _sequencer;

        public DialogEntryPoint
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
                success: _ =>
                {
                    _logger.LogInformation("Успешно завершено!");

                    return 1;
                },
                expected: _ =>
                {
                    _logger.LogWarning("Выполнение было отменено!");

                    return 2;
                },
                unexpected: exception =>
                {
                    _logger.LogException(exception);

                    return 3;
                }
            );
        }
    }
}
