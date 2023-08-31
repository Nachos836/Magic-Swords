﻿using System;

namespace MagicSwords.Features.AnimatedRichText.Animating.Trigger
{
    internal sealed record TriggerEffect : IEffect
    {
        Tween IEffect.Tween { get; }

        object ICloneable.Clone()
        {
            return this with { };
        }
    }
}
