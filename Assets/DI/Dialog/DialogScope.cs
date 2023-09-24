using System;
using AnySerialize;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog
{
    using Common;
    using Dependencies;
    using Features.Dialog;

    internal sealed class DialogScope : LifetimeScope
    {
        [field: SerializeField] public TextMeshProUGUI Field { get; private set; }
        [AnySerialize] [UsedImplicitly] public TimeSpan SymbolsDelay { get; }
        [AnySerialize] [UsedImplicitly] public TimeSpan MessagesDelay { get; }
        [field: SerializeField] public string[] Monologue { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddLogger(out var logger)
                .AddScopeEntry<DialogEntryPoint>(logger)
                .AddAnimatedTextPresenter(Field, SymbolsDelay, MessagesDelay, Monologue);
        }
    }
}
