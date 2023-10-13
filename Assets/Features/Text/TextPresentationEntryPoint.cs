using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer.Unity;

namespace MagicSwords.Features.Text
{
    using Generic.Functional;
    using Logger;

    internal sealed class TextPresentationEntryPoint : IAsyncStartable
    {
        private readonly PlayerLoopTiming _initiatingPoint;
        private readonly IBufferedAsyncSubscriber<IPresentJob> _textTargets;
        private readonly ILogger _logger;

        public TextPresentationEntryPoint(PlayerLoopTiming initiatingPoint, IBufferedAsyncSubscriber<IPresentJob> textTargets, ILogger logger)
        {
            _initiatingPoint = initiatingPoint;
            _textTargets = textTargets;
            _logger = logger;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await UniTask.Yield(_initiatingPoint, cancellation);

            PresentationRoutineAsync(_textTargets, cancellation)
                .Forget();
        }

        private async UniTaskVoid PresentationRoutineAsync(IBufferedAsyncSubscriber<IPresentJob> textTargets, CancellationToken cancellation = default)
        {
            var result = AsyncResult.Success;
            var presentationDone = false;

            using var _ = await textTargets.SubscribeAsync(async (job, token) =>
            {
                result = result.Combine(await job.PresentAsync(token));
                presentationDone = true;

            }, cancellation);

            await UniTask.WaitUntil(() => presentationDone, _initiatingPoint, cancellation);

            await result.MatchAsync
            (
                success: static _ => UniTask.CompletedTask,
                cancellation: static _ => UniTask.CompletedTask,
                failure: (exception, _) =>
                {
                    _logger.LogException(exception);

                    return UniTask.CompletedTask;
                },
                cancellation
            );
        }
    }
}
