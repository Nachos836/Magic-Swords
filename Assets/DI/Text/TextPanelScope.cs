using System;
using AnySerialize;
using JetBrains.Annotations;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    internal sealed class TextPanelScope : LifetimeScope
    {
        [SerializeField] [ReadOnly] private TMP_Text? _field;
        [SerializeField] private PresentationConfig _config = new ();

        private void OnValidate() => _field ??= GetComponentInChildren<TMP_Text>();

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_field!);
            builder.RegisterInstance(_config)
                .As<ISymbolsDelay, IMessagesDelay>();
        }
    }

    internal interface ISymbolsDelay
    {
        TimeSpan Value { get; }
    }

    internal interface IMessagesDelay
    {
        TimeSpan Value { get; }
    }

    [Serializable]
    internal sealed class PresentationConfig : ISymbolsDelay, IMessagesDelay
    {
        [AnySerialize] [UsedImplicitly] private TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] private TimeSpan MessagesDelay { get; }

        TimeSpan ISymbolsDelay.Value => SymbolsDelay;
        TimeSpan IMessagesDelay.Value => MessagesDelay;
    }
}
