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
        [field: SerializeField] public AnimatedTextPresenter Presenter { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Field { get; private set; }
        [AnySerialize] [UsedImplicitly] public TimeSpan Delay { get; }
        [field: SerializeField] public string[] Monologue { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.AddAnimatedTextPresenter(Presenter, Field, Delay, Monologue);
        }
    }
}
