using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.Generic.Sequencer
{
    using Functional;
    using Functional.Outcome;

    public sealed class Sequencer
    {
        private readonly IStage _firstState;

        public Sequencer(IStage firstState) => _firstState = firstState;

        public async UniTask<Result<Success, Expected.Cancellation>> StartAsync(CancellationToken cancellation = default)
        {
            var current = _firstState;

            while (current is not Stage.Ended)
            {
                if (cancellation.IsCancellationRequested) return Expected.Canceled;
                if (current is not IStage.IProcess candidate) return Unexpected.Error;

                if (candidate is IAsyncStartable startable) await startable.StartAsync(cancellation);

                var outcome = await candidate.ProcessAsync(cancellation);
                var option = await outcome.MatchAsync
                (
                    (next, token) => token.IsCancellationRequested
                        ? new UniTask<IStage>(Stage.Cancel)
                        : new UniTask<IStage>(current = next),
                    (cancelled, _) => new UniTask<IStage>(cancelled),
                    (error, _) => new UniTask<IStage>(error),
                    cancellation
                );

                if (candidate is IUniTaskAsyncDisposable disposable) await disposable.DisposeAsync();

                if (cancellation.IsCancellationRequested) return Expected.Canceled;

                switch (option)
                {
                    case Stage.Ended: return Expected.Success;
                    case Stage.Canceled: return Expected.Canceled;
                    case Stage.Errored: return Unexpected.Failed;
                }
            }

            return Expected.Success;
        }
    }
}
