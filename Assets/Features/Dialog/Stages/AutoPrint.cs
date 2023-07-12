using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.Generic.Functional;
using TMPro;

using static System.Threading.CancellationTokenSource;
using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.Features.Dialog.Stages
{
    using Generic.Sequencer;

    using Option = OneOf
    <
        Generic.Sequencer.IStage,
        Generic.Sequencer.Stage.Canceled,
        Generic.Sequencer.Stage.Errored
    >;

    internal sealed class AutoPrint : IStage, IStage.IProcess
    {
        private readonly string _text;
        private readonly TextMeshProUGUI _field;
        private readonly TimeSpan _delay;

        public AutoPrint(string text, TextMeshProUGUI field, TimeSpan delay)
        {
            _text = text;
            _field = field;
            _delay = delay;
        }

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            var displaying = CreateLinkedTokenSource(cancellation);
            cancellation = displaying.Token;

            await foreach (var _ in EveryUpdate(FixedUpdate).TakeUntilCanceled(cancellation).WithCancellation(cancellation))
            {
                for (var i = 0; i < _text.Length; i++)
                {
                    if (cancellation.IsCancellationRequested) return Option.From(Stage.Cancel);

                    _field.text = _text[..i];

                    if (await UniTask.Delay(_delay, ignoreTimeScale: false, FixedUpdate, cancellation)
                        .SuppressCancellationThrow()) return Option.From(Stage.Cancel);

                    if (await UniTask.Yield(FixedUpdate, cancellation)
                        .SuppressCancellationThrow()) return Option.From(Stage.Cancel);
                }

                displaying.Cancel();
            }

            return Option.From(Stage.End);
        }
    }
}