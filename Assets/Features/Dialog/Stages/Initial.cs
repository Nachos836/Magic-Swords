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
        private readonly Func<Message, Print> _autoPrintResolver;
        private readonly string[] _monologue;

        public Initial(Func<Message, Print> autoPrintResolver, string[] monologue)
        {
            _autoPrintResolver = autoPrintResolver;
            _monologue = monologue;
        }

        UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return new UniTask<Option>(Stage.Cancel);

            return new UniTask<Option>(Option.From(_autoPrintResolver.Invoke(new Message(_monologue))));
        }
    }
}
