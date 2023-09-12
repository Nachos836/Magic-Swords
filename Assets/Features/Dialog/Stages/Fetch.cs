using System;
using System.Threading;
using Cysharp.Threading.Tasks;

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

    internal sealed class Fetch : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly Message.Fetcher _fetcher;

        public Fetch(Func<Message, IStage> resolveNext, Message.Fetcher fetcher)
        {
            _resolveNext = resolveNext;
            _fetcher = fetcher;
        }

        UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return new UniTask<Option>(Option.From(Stage.Cancel));

            return new UniTask<Option>(Option.From(_fetcher.Next.Match
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
