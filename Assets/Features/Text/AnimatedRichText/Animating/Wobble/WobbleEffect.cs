using System;
using Unity.Mathematics;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating.Wobble
{
    [Serializable]
    internal sealed record WobbleEffect : IEffect
    {
        [SerializeField] private float _strength = 0.01f;
        [SerializeField] private float _amplitude = 10.0f;

        Tween IEffect.Tween => Tween;

        internal IEffect Configure(float strength, float amplitude) => new WobbleEffect
        {
            _strength = strength,
            _amplitude = amplitude
        };

        public static Vector3 GenericTween(Vector3 origin, float t, float strength = 0.01f, float amplitude = 2.0f)
        {
            var genericTween = new Vector3
            (
                x: math.cos(t + origin.x * strength) * amplitude,
                y: math.sin(t + origin.x * strength) * amplitude,
                z: math.tan(t + origin.x * strength) * amplitude
            );

            return genericTween;
        }

        private Vector3 Tween(Vector3 origin, float t) => GenericTween(origin, t, _strength, _amplitude);

        object ICloneable.Clone() => this with { };
    }
}
