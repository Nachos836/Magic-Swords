using System;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating.Trigger
{
    internal sealed record TriggerEffect : IEffect
    {
        Tween IEffect.Tween { get; } = NoneEffect.Instance.Tween;

        object ICloneable.Clone() => this with { };
    }
}
