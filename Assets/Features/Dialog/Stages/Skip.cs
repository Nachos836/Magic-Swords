using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

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

    internal sealed class Skip : IStage, IStage.IProcess
    {
        private readonly IInputFor<SubmitAction> _submitAction;
        private readonly PlayerLoopTiming _yieldTarget;
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;
        private readonly TextMeshProUGUI _text;

        public Skip
        (
            IInputFor<SubmitAction> submitAction,
            PlayerLoopTiming yieldTarget,
            Func<Message, IStage> resolveNext,
            Message message,
            TextMeshProUGUI text
        ) {
            _submitAction = submitAction;
            _yieldTarget = yieldTarget;
            _resolveNext = resolveNext;
            _message = message;
            _text = text;
        }

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return Stage.Cancel;

            _text.text = _message.Part;
            var skipPerformed = false;

            using var _ = _submitAction.Subscribe
            (
                started: _ => skipPerformed = true,
                performed: _ => skipPerformed = true,
                canceled: _ => { }
            );

            if (await UniTask.WaitUntil
            (
                predicate: () => skipPerformed,//Mouse.current.leftButton.wasPressedThisFrame,
                timing: _yieldTarget,
                cancellation
            ).SuppressCancellationThrow()) return Stage.Cancel;

            return Option.From(_resolveNext.Invoke(_message));
        }
    }
}
