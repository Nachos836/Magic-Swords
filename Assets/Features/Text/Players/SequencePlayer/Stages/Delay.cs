using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.Players.SequencePlayer.Stages
{
    using Generic.Sequencer;
    using Generic.Functional;
    using Payload;

    internal sealed class Delay : IStage, IStage.IProcess
    {
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;
        private readonly TimeSpan _amount;

        public Delay
        (
            PlayerLoopTiming yieldTarget,
            Func<Message, IStage> resolveNext,
            Message message,
            TimeSpan amount
        ) {
            _yieldTarget = yieldTarget;
            _resolveNext = resolveNext;
            _message = message;
            _amount = amount;
        }

        async UniTask<AsyncResult<IStage>> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return AsyncResult<IStage>.Cancel;

            if (await UniTask.Delay(_amount, ignoreTimeScale: false, _yieldTarget, cancellation)
                .SuppressCancellationThrow()) return AsyncResult<IStage>.Cancel;

            return AsyncResult<IStage>.FromResult(_resolveNext.Invoke(_message));
        }
    }
}
