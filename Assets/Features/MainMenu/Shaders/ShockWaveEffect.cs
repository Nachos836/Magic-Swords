using System;
using System.Threading;
using AnySerialize;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

using static Cysharp.Threading.Tasks.DelayType;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.Features.MainMenu.Shaders
{
    internal sealed class ShockWaveEffect : MonoBehaviour
    {
        private static readonly (double Start, double End) WaveFactor = (-0.1, 1.0);
        private static readonly int DistanceFromCenter = Shader.PropertyToID("_DistanceFromCenter");
        private static readonly int RingSpawnPosition = Shader.PropertyToID("_RingSpawnPosition");

        [UsedImplicitly] [AnySerialize] private TimeSpan WaveDuration { get; }
        [UsedImplicitly] [AnySerialize] private TimeSpan IterationDelay { get; }
        [SerializeField] private Material _material;
        [SerializeField] private RectTransform _startPosition;

        private Vector4 _savedRingSpawnPosition;
        private float _savedDistanceFromCenter;

        [UsedImplicitly] // ReSharper disable once Unity.IncorrectMethodSignature
        private UniTaskVoid Awake()
        {
            _savedRingSpawnPosition = _material.GetVector(RingSpawnPosition);
            _savedDistanceFromCenter = _material.GetFloat(DistanceFromCenter);
            var newRingSpawnPosition = (_startPosition.anchorMax + _startPosition.anchorMin) / 2;

            _material.SetVector(RingSpawnPosition, newRingSpawnPosition);

            return EffectLoopAsync(destroyCancellationToken);

            async UniTaskVoid EffectLoopAsync(CancellationToken cancellation = default)
            {
                while (cancellation.IsCancellationRequested is false)
                {
                    await RoutineAsync(WaveFactor, cancellation);

                    await UniTask.Delay(IterationDelay, Realtime, FixedUpdate, cancellation)
                        .SuppressCancellationThrow();
                }

                return;

                async UniTask RoutineAsync((double Start, double End) waveFactor, CancellationToken token = default)
                {
                    if (token.IsCancellationRequested) return;

                    _material.SetFloat(DistanceFromCenter, (float) waveFactor.Start);

                    var elapsedTime = TimeSpan.Zero;

                    while (elapsedTime < WaveDuration && token.IsCancellationRequested is false)
                    {
                        var normalizedTime = elapsedTime / WaveDuration;
                        var wave = (float) math.lerp(waveFactor.Start, waveFactor.End, normalizedTime);
                        _material.SetFloat(DistanceFromCenter, wave);

                        await UniTask.Yield(FixedUpdate, token)
                            .SuppressCancellationThrow();

                        elapsedTime += TimeSpan.FromSeconds(Time.fixedDeltaTime);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _material.SetVector(RingSpawnPosition, _savedRingSpawnPosition);
            _material.SetFloat(DistanceFromCenter, _savedDistanceFromCenter);
        }
    }
}
