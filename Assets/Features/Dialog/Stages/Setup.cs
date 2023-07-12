using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace MagicSwords.Features.Dialog.Stages
{
    using Generic.Sequencer;

    using Option = Generic.Functional.OneOf
    <
        Generic.Sequencer.IStage,
        Generic.Sequencer.Stage.Canceled,
        Generic.Sequencer.Stage.Errored
    >;

    internal sealed class Setup : IStage, IStage.IProcess
    {
        private readonly Func<string, TextMeshProUGUI, TimeSpan, AutoPrint> _autoPrintResolver;
        private readonly TextMeshProUGUI _field;
        private readonly TimeSpan _delay;
        private readonly string[] _monologue;

        public Setup
        (
            Func<string, TextMeshProUGUI, TimeSpan, AutoPrint> autoPrintResolver,
            TextMeshProUGUI field,
            float delay,
            string[] monologue
        ) {
            _autoPrintResolver = autoPrintResolver;
            _field = field;
            _delay = TimeSpan.FromMilliseconds(delay);
            _monologue = monologue;
        }

        UniTask<Option> IStage.IProcess.ProcessAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return new UniTask<Option>
            (
                Stage.Cancel
            );

            return new UniTask<Option>(Option.From(_autoPrintResolver.Invoke(_monologue[0], _field, _delay)));
        }
    }
}
