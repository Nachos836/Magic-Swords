using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.InputSystem;

using static Cysharp.Threading.Tasks.PlayerLoopTiming;

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

    public sealed class Skip : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;
        private readonly TextMeshProUGUI _text;

        public Skip(Func<Message, IStage> resolveNext, Message message, TextMeshProUGUI text)
        {
            _resolveNext = resolveNext;
            _message = message;
            _text = text;
        }

        async UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return Stage.Cancel;

            _text.text = _message.Part;

            if (await UniTask.WaitUntil
            (
                predicate: () => Mouse.current.leftButton.wasPressedThisFrame,
                timing: Update,
                cancellation
            ).SuppressCancellationThrow()) return Stage.Cancel;

            return Option.From(_resolveNext.Invoke(_message));
        }
    }
}
