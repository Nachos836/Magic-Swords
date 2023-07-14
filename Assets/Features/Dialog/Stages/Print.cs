using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static System.Threading.CancellationTokenSource;
using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.Features.Dialog.Stages
{
    using Generic.Sequencer;
    using Payload;

    using Option = Generic.Functional.OneOf
    <
        Generic.Sequencer.IStage,
        Generic.Sequencer.Stage.Canceled,
        Generic.Sequencer.Stage.Errored
    >;

    public sealed class Print : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;
        private readonly TextMeshProUGUI _field;
        private readonly TimeSpan _delay;

        public Print(Func<Message, IStage> resolveNext, Message message, TextMeshProUGUI field, TimeSpan delay)
        {
            _resolveNext = resolveNext;
            _message = message;
            _field = field;
            _delay = delay;
        }

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            var displaying = CreateLinkedTokenSource(cancellation);
            cancellation = displaying.Token;

            await foreach (var _ in EveryUpdate(EarlyUpdate).TakeUntilCanceled(cancellation).WithCancellation(cancellation))
            {
                var message = _message.Part;

                for (var i = 0; i < message.Length; i++)
                {
                    if (cancellation.IsCancellationRequested) return Option.From(Stage.Cancel);

                    _field.text = message[..i];

                    if (await UniTask.Delay(_delay, ignoreTimeScale: true, EarlyUpdate, cancellation)
                        .SuppressCancellationThrow()) return Option.From(Stage.Cancel);

                    if (await UniTask.Yield(EarlyUpdate, cancellation)
                        .SuppressCancellationThrow()) return Option.From(Stage.Cancel);
                }

                displaying.Cancel();
            }

            return Option.From(_resolveNext.Invoke(_message));
        }
    }
}
