using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static System.Threading.CancellationTokenSource;
using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.Dialog.Stages
{
    using Generic.Sequencer;
    using Input;
    using Payload;

    using Option = Generic.Functional.OneOf
    <
        Generic.Sequencer.IStage,
        Generic.Sequencer.Stage.Canceled,
        Generic.Sequencer.Stage.Errored
    >;

    internal sealed class Print : IStage, IStage.IProcess
    {
        private readonly IInputFor<UISubmission> _uiSubmissionInput;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Func<Message, IStage> _resolveSkip;
        private readonly Message _message;
        private readonly TextMeshProUGUI _field;
        private readonly TimeSpan _delay;

        public Print
        (
            IInputFor<UISubmission> uiSubmissionInput,
            PlayerLoopTiming yieldTarget,
            Func<Message, IStage> resolveNext,
            Func<Message, IStage> resolveSkip,
            Message message,
            TextMeshProUGUI field,
            TimeSpan delay
        ) {
            _uiSubmissionInput = uiSubmissionInput;
            _yieldTarget = yieldTarget;
            _resolveNext = resolveNext;
            _resolveSkip = resolveSkip;
            _message = message;
            _field = field;
            _delay = delay;
        }

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
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
                    if (cancellation.IsCancellationRequested) return Option.From(Stage.Cancel);

                    _field.text = message[..i];

                    var (wasCanceled, finishedIndex) = await UniTask.WhenAny
                    (
                        UniTask.WaitUntil(() => skipPerformed, _yieldTarget, cancellation),
                        UniTask.Delay(_delay, ignoreTimeScale: false, _yieldTarget, cancellation)
                    ).SuppressCancellationThrow();

                    if (wasCanceled)
                    {
                        return Option.From(Stage.Cancel);
                    }
                    else if (finishedIndex is 0)
                    {
                        displaying.Cancel();

                        return Option.From(_resolveSkip.Invoke(_message));
                    }
                }

                displaying.Cancel();
            }

            return Option.From(_resolveNext.Invoke(_message));

            async UniTaskVoid HandleInputAsync(CancellationToken token = default)
            {
                using var _ = _uiSubmissionInput.Subscribe
                (
                    started: _ => skipPerformed = true,
                    performed: _ => skipPerformed = true,
                    canceled: _ => { }
                );

                await UniTask
                    .WaitUntil(() => skipPerformed, _yieldTarget, token)
                    .SuppressCancellationThrow();
            }
        }
    }
}
