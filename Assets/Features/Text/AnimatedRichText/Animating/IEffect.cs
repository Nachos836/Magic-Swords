using System;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating
{
    internal interface IEffect : ICloneable
    {
        Tween Tween { get; }
    }

    internal sealed class NoneEffect : IEffect
    {
        internal static IEffect Instance { get; } = new NoneEffect();

        object ICloneable.Clone() => Instance;
        Tween IEffect.Tween { get; } = static (origin, _) => origin;
    }
}
