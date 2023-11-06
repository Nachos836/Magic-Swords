using System;
using System.Linq;
using AnySerialize;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

using static Cysharp.Threading.Tasks.PlayerLoopTiming;

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
        [ValidateInput(Validation.OfAssetReferenceGameObject)]
        [SerializeField] private AssetReferenceGameObject _panelScope = AssetReferenceGameObjectEmpty.Instance;
        [SerializeField] private RichText[] _replicas = Array.Empty<RichText>();

        [field: Header("Presentation Options:")]

        [AnySerialize] [UsedImplicitly] private TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] private TimeSpan MessagesDelay { get; }

        private IText[] _preparedReplicas = Array.Empty<IText>();

        protected override void Awake()
        {
            _preparedReplicas = _replicas.Select(static piece => (ITextWithPreWarm) piece)
                .Select(piece => piece.PreWarmLazyAsync(destroyCancellationToken))
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
                    loadPoint: Initialization,
                    animationPoint: FixedUpdate,
                    _preparedReplicas

                ), Lifetime.Scoped).As<ITextPanel>();
        }
    }
}
