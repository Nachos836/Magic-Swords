using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static System.Threading.CancellationTokenSource;
using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.Text.Players.SequencePlayer.Stages
{
    using Generic.Sequencer;
    using Generic.Functional;
    using Input;
    using Payload;

    internal sealed class Print : IStage, IStage.IProcess
    {
        private readonly IInputFor<ReadingSkip> _readingSkipInput;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Func<Message, IStage> _resolveSkip;
        private readonly Message _message;
        private readonly TextMeshProUGUI _field;
        private readonly TimeSpan _delay;

        public Print
        (
            IInputFor<ReadingSkip> readingSkipInput,
            PlayerLoopTiming yieldTarget,
            Func<Message, IStage> resolveNext,
            Func<Message, IStage> resolveSkip,
            Message message,
            TextMeshProUGUI field,
            TimeSpan delay
        ) {
            _readingSkipInput = readingSkipInput;
            _yieldTarget = yieldTarget;
            _resolveNext = resolveNext;
            _resolveSkip = resolveSkip;
            _message = message;
            _field = field;
            _delay = delay;
        }

        async UniTask<AsyncResult<IStage>> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            using var displaying = CreateLinkedTokenSource(cancellation);
            cancellation = displaying.Token;
            var skipPerformed = false;

            HandleInputAsync(cancellation).Forget();

            await foreach (var _ in EveryUpdate(_yieldTarget).TakeUntilCanceled(cancellation))
            {
                var message = _message.Part;

                for (var i = 0; i < message.Length; i++)
                {
                    if (cancellation.IsCancellationRequested) return AsyncResult<IStage>.Cancel;

                    _field.text = message[..i];

                    var (wasCanceled, finishedIndex) = await UniTask.WhenAny
                    (
                        UniTask.WaitUntil(WasSkipped, _yieldTarget, cancellation),
                        UniTask.Delay(_delay, ignoreTimeScale: false, _yieldTarget, cancellation)
                    ).SuppressCancellationThrow();

                    if (wasCanceled)
                    {
                        return AsyncResult<IStage>.Cancel;
                    }
                    else if (finishedIndex is 0)
                    {
                        displaying.Cancel();

                        return AsyncResult<IStage>.FromResult(_resolveSkip.Invoke(_message));
                    }
                }

                displaying.Cancel();
            }

            return AsyncResult<IStage>.FromResult(_resolveNext.Invoke(_message));

            bool WasSkipped() => skipPerformed;

            async UniTaskVoid HandleInputAsync(CancellationToken token = default)
            {
                using var _ = _readingSkipInput.Subscribe(started: PerformSkip);

                await UniTask.WaitUntil(WasSkipped, _yieldTarget, token)
                    .SuppressCancellationThrow();

                return;

                void PerformSkip(StartedContext _) => skipPerformed = true;
            }
        }
    }
}
