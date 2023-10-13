using System;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating
{
    internal interface IEffect : ICloneable
    {
        Tween Tween { get; }
    }
}
