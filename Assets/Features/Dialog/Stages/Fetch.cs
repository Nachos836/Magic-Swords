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

    public sealed class Fetch : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message _message;

        public Fetch(Func<Message, IStage> resolveNext, Message message)
        {
            _resolveNext = resolveNext;
            _message = message;
        }

        UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return new UniTask<Option>(Option.From(Stage.Cancel));

            return new UniTask<Option>(Option.From(_message.Next.Match
            (
                something: message => cancellation.IsCancellationRequested
                    ? Stage.Cancel
                    : _resolveNext.Invoke(message),
                nothing: () => Stage.End,
                failure: _ => Stage.Error
            )));
        }
    }
}
