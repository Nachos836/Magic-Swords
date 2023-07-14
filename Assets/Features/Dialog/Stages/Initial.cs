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

    public sealed class Initial : IStage, IStage.IProcess
    {
        private readonly Func<Message, IStage> _resolveNext;
        private readonly string[] _monologue;

        public Initial(Func<Message, IStage> resolveNext, string[] monologue)
        {
            _resolveNext = resolveNext;
            _monologue = monologue;
        }

        UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return new UniTask<Option>(Stage.Cancel);

            return new UniTask<Option>(Option.From(_resolveNext.Invoke(new Message(_monologue))));
        }
    }
}
