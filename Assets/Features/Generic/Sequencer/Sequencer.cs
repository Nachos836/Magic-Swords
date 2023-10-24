using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.Sequencer
{
    using Functional;

    public sealed class Sequencer
    {
        private readonly IStage _firstState;

        public Sequencer(IStage firstState) => _firstState = firstState;

        public async UniTask<AsyncResult> StartAsync(CancellationToken cancellation = default)
        {
            var current = _firstState;

            while (current is not Stage.Ended)
            {
                if (cancellation.IsCancellationRequested) return AsyncResult.Cancel;
                if (current is not IStage.IProcess candidate) return AsyncResult.Impossible;

                var outcome = await candidate.ProcessAsync(cancellation);
                var goingForNextStage = outcome.Run((next, tokens) =>
                {
                    if (tokens.IsCancellationRequested) return AsyncResult.Cancel;

                    current = next;

                    return AsyncResult.Success;

                }, cancellation).IsSuccessful;

                if (goingForNextStage is false) return outcome.Match
                (
                    success: static (_, _) => AsyncResult.Impossible,
                    cancellation: static _ => AsyncResult.Cancel,
                    error: static (exception, _) => AsyncResult.FromException(exception),
                    cancellation
                );
            }

            return AsyncResult.Success;
        }
    }
}
