using System;
using UnityEngine;

namespace MagicSwords.Features.TextAnimator.Effect.Variants
{
    internal sealed class WobbleEffect : IEffect
    {
        private readonly Func<float> _fetchCurrentTime;

        public WobbleEffect(Func<float> fetchCurrentTime)
        {
            _fetchCurrentTime = fetchCurrentTime;
        }

        string IEffect.Name => "wobble";
        Tween IEffect.Tween => WobbleTween;

        private Vector3 WobbleTween(Vector3 origin)
        {
            var time = _fetchCurrentTime.Invoke();

            return new Vector3
            (
                x: Mathf.Cos(time * 2f + origin.x * 0.01f) * 10f,
                y: Mathf.Sin(time * 2f + origin.x * 0.01f) * 10f,
                z: Mathf.Tan(time * 2f + origin.x * 0.01f) * 10f
            );
        }
    }
}
