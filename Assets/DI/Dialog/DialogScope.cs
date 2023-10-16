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
        [SerializeField] private AssetReferenceGameObject _panelScope;
        [SerializeField] private SequencedText _text;

        [field: Header("Presentation Options:")]

        [AnySerialize] [UsedImplicitly] private TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] private TimeSpan MessagesDelay { get; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddScopeEntry<DialogEntryPoint>()
                .AddUIInput()
                .AddReadingInput()
                .Register(resolver => new TextUIPanel
                (
                    _panelScope,
                    parent: this,
                    yieldPoint: PlayerLoopTiming.Initialization,
                    resolver.Resolve<MessagePipeOptions>(),
                    _text

                ), Lifetime.Scoped).As<ITextPanel>();
        }
    }
}
