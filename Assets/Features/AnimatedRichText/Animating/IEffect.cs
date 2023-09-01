using System;

namespace MagicSwords.Features.AnimatedRichText.Animating
{
    internal interface IEffect : ICloneable
    {
        Tween Tween { get; }
    }
}
