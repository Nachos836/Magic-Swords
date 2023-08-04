using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Dialog.Stages
{
    using Payload;
    using Generic.Sequencer;

    using Option = Generic.Functional.OneOf
    <
        Generic.Sequencer.IStage,
        Generic.Sequencer.Stage.Canceled,
        Generic.Sequencer.Stage.Errored
    >;

    public sealed class Delay : IStage, IStage.IProcess
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

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return Stage.Cancel;

            if (await UniTask.Delay(_amount, ignoreTimeScale: false, _yieldTarget, cancellation)
                .SuppressCancellationThrow()) return Stage.Cancel;

            return Option.From(_resolveNext.Invoke(_message));
        }
    }
}
