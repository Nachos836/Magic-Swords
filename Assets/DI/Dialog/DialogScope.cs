using System;
using AnySerialize;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog
{
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

            builder.RegisterEntryPoint<DialogEntryPoint>(Lifetime.Scoped);
            builder.RegisterEntryPointExceptionHandler(Handlers.DefaultExceptionHandler);

            builder.AddAnimatedTextPresenter(Field, SymbolsDelay, MessagesDelay, Monologue);
        }
    }
}
