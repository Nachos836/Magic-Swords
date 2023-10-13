using System;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating.Wobble
{
    [Serializable]
    internal sealed record WobbleEffect : IEffect
    {
        [SerializeField] private float _strength = 0.01f;
        [SerializeField] private float _amplitude = 10.0f;

        Tween IEffect.Tween => WobbleTween;

        private Vector3 WobbleTween(Vector3 origin, float t)
        {
            return new Vector3
            (
                x: Mathf.Cos(t * 2f + origin.x * _strength) * _amplitude,
                y: Mathf.Sin(t * 2f + origin.x * _strength) * _amplitude,
                z: Mathf.Tan(t * 2f + origin.x * _strength) * _amplitude
            );
        }

        object ICloneable.Clone()
        {
            return this with { };
        }
    }
}
