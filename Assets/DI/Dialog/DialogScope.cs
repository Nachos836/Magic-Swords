using MagicSwords.Features.Dialog;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog
{
    using Dependencies;

    internal sealed class DialogScope : LifetimeScope
    {
        [field: SerializeField] public AnimatedTextPresenter Presenter { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Field { get; private set; }
        [field: SerializeField] public float Delay { get; private set; } = 0.05f;
        [field: SerializeField] public string[] Monologue { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.AddAnimatedTextPresenter(Presenter, Field, Delay, Monologue);
        }
    }
}
