using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace MagicSwords.Features.Text.Players.SequencePlayer.Stages
{
    using Generic.Sequencer;
    using Generic.Functional;
    using Input;
    using Payload;

    internal sealed class Skip : IStage, IStage.IProcess
    {
        private readonly IInputFor<ReadingSkip> _readingSkipInput;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;
        private readonly TextMeshProUGUI _text;

        public Skip
        (
            IInputFor<ReadingSkip> readingSkipInput,
            PlayerLoopTiming yieldTarget,
            Func<Message, IStage> resolveNext,
            Message message,
            TextMeshProUGUI text
        ) {
            _readingSkipInput = readingSkipInput;
            _yieldTarget = yieldTarget;
            _resolveNext = resolveNext;
            _message = message;
            _text = text;
        }

        async UniTask<AsyncResult<IStage>> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return AsyncResult<IStage>.Cancel;

            _text.text = _message.Part;
            var skipPerformed = false;

            using var _ = _readingSkipInput.Subscribe(started: PerformSkip);

            if (await UniTask.WaitUntil(WasSkipped, _yieldTarget, cancellation)
                .SuppressCancellationThrow()) return AsyncResult<IStage>.Cancel;

            return AsyncResult<IStage>.FromResult(_resolveNext.Invoke(_message));

            void PerformSkip(StartedContext _) => skipPerformed = true;
            bool WasSkipped() => skipPerformed;
        }
    }
}
