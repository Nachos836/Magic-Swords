using System;
using System.Linq;
using AnySerialize;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog
{
    using Common;
    using Text;
    using Features.Dialog;
    using Features.Text;
    using Features.Text.UI;
    using Features.Text.AnimatedRichText;

    internal sealed class DialogScope : LifetimeScope
    {
        [SerializeField] private AssetReferenceGameObject _panelScope;
        [SerializeField] private RichText[] _replicas;

        [field: Header("Presentation Options:")]

        [AnySerialize] [UsedImplicitly] private TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] private TimeSpan MessagesDelay { get; }

        private IText[] _preparedReplicas = Array.Empty<IText>();

        protected override void Awake()
        {
            _preparedReplicas = _replicas.Select(static piece => (ITextWithPrewarm) piece)
                .Select(piece => piece.PrewarmLazyAsync(destroyCancellationToken))
                .ToArray();

            _preparedReplicas = _preparedReplicas.Length is not 0
                ? _preparedReplicas
                : _replicas.Cast<IText>()
                    .ToArray();

            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddScopeEntry<DialogEntryPoint>()
                .AddUIInput()
                .AddReadingInput()
                .Register(_ => new TextUIPanel
                (
                    _panelScope,
                    parent: this,
                    yieldPoint: PlayerLoopTiming.Initialization,
                    _replicas.First()

                ), Lifetime.Scoped).As<ITextPanel>();
        }
    }
}
