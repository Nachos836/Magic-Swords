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

    internal sealed class DialogScope : LifetimeScope
    {
        [field: SerializeField] public TextMeshProUGUI Field { get; private set; }
        [AnySerialize] [UsedImplicitly] public TimeSpan Delay { get; }
        [field: SerializeField] public string[] Monologue { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.AddAnimatedTextPresenter(Field, Delay, Monologue);
        }
    }
}
