using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing.Stages
{
    using Generic.Functional;
    using Generic.Sequencer;
    using Payload;

    internal sealed class Fetch : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message.Fetcher _fetcher;

        public Fetch(Func<Message, IStage> resolveNext, Message.Fetcher fetcher)
        {
            _resolveNext = resolveNext;
            _fetcher = fetcher;
        }

        UniTask<AsyncResult<IStage>> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(AsyncResult<IStage>.Cancel);

            return UniTask.FromResult
            (
                _fetcher.Next.Attach(_resolveNext).Run
                (
                    whenSome: static (message, resolver, token) => token.IsCancellationRequested is false
                        ? AsyncResult<IStage>.FromResult(resolver.Invoke(message))
                        : AsyncResult<IStage>.Cancel,
                    whenNone: static token => token.IsCancellationRequested is false
                        ? AsyncResult<IStage>.FromResult(Stage.End)
                        : AsyncResult<IStage>.Cancel,
                    cancellation
                )
            );
        }
    }
}
