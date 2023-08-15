using System;

namespace MagicSwords.Features.TextAnimator.Effect
{
    internal interface IEffect
    {
        string Name { get; }
        Tween Tween { get; }

        bool TryMatch(string candidate)
        {
            return string.Equals(Name, candidate, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
