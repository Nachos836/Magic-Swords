using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.Players.SequencePlayer.Stages
{
    using Generic.Sequencer;
    using Generic.Functional;
    using Payload;

    internal sealed class Initial : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly string[] _monologue;

        public Initial(Func<Message, IStage> resolveNext, string[] monologue)
        {
            _resolveNext = resolveNext;
            _monologue = monologue;
        }

        UniTask<AsyncResult<IStage>> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            return UniTask.FromResult
            (
                cancellation.IsCancellationRequested
                    ? AsyncResult<IStage>.Cancel
                    : AsyncResult<IStage>.FromResult(_resolveNext.Invoke(new Message(_monologue)))
            );
        }
    }
}
