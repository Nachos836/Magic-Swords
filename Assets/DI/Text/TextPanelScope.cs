using System;
using AnySerialize;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Text
{
    internal sealed class TextPanelScope : LifetimeScope
    {
        [SerializeField] private TMP_Text _field;
        [SerializeField] private PresentationConfig _config;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_field);
            builder.RegisterInstance(_config)
                .As<ISymbolsDelay, IMessagesDelay>();
        }

        private void OnValidate()
        {
            _field ??= GetComponentInChildren<TMP_Text>();
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
        [AnySerialize] [UsedImplicitly] private TimeSpan SymbolsDelay { get; } = TimeSpan.FromMilliseconds(10);
        [AnySerialize] [UsedImplicitly] private TimeSpan MessagesDelay { get; } = TimeSpan.FromMilliseconds(300);

        TimeSpan ISymbolsDelay.Value => SymbolsDelay;
        TimeSpan IMessagesDelay.Value => MessagesDelay;
    }
}
