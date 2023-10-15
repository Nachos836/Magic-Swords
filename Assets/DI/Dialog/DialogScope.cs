using System;
using AnySerialize;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MessagePipe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog
{
    using Common;
    using Text;
    using Features.Dialog;
    using Features.Text.UI;
    using Features.Text;

    internal sealed class DialogScope : LifetimeScope
    {
        [AnySerialize] [UsedImplicitly] public TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] public TimeSpan MessagesDelay { get; }
        [field: SerializeField] public string[] Monologue { get; private set; }

        [SerializeField] private AssetReferenceGameObject _panel;
        [SerializeField] private SequencedText _text;

        [SerializeField] private AssetReferenceGameObject _panelScope;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddUnityBasedLogger(out var logger)
                .AddScopeEntry<DialogEntryPoint>(logger)
                // .AddAnimatedTextPresenter(Field, SymbolsDelay, MessagesDelay, Monologue)
                .AddUIInput()
                .AddReadingInput()
                .Register(_ => new TextUIPanel
                (
                    _panelScope,
                    parent: this,
                    _panel,
                    yieldPoint: PlayerLoopTiming.Initialization,
                    new MessagePipeOptions(),
                    _text

                ), Lifetime.Scoped).As<ITextPanel>();
        }
    }
}
