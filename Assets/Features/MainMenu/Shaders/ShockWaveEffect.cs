using System;
using System.Threading;
using AnySerialize;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

using static Cysharp.Threading.Tasks.DelayType;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.Features.MainMenu.Shaders
{
    using static Generic.ExtendDotNet.CancellationTokenSourceExtensions;

    [RequireComponent(typeof(Graphic))]
    internal sealed class ShockWaveEffect : MonoBehaviour
    {
        private static readonly (double Start, double End) WaveFactor = (-0.1, 1.0);
        private static readonly int DistanceFromCenter = Shader.PropertyToID("_DistanceFromCenter");
        private static readonly int RingSpawnPosition = Shader.PropertyToID("_RingSpawnPosition");

        [SerializeField] [HideInInspector] private Graphic _graphic;
        [UsedImplicitly] [AnySerialize] private TimeSpan WaveDuration { get; }
        [UsedImplicitly] [AnySerialize] private TimeSpan IterationDelay { get; }
        [SerializeField] private RectTransform _startPosition = new ();

        private CancellationTokenSource _processing = new ();

        private void OnValidate() => _graphic = GetComponent<Graphic>();

        private void Awake()
        {
            _graphic.material = new Material(_graphic.material){ enableInstancing = true };
            var newRingSpawnPosition = (_startPosition.anchorMax + _startPosition.anchorMin) / 2;

            _graphic.materialForRendering.SetVector(RingSpawnPosition, newRingSpawnPosition);
        }

        [UsedImplicitly] // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid OnEnable()
        {
            _processing.AddTo(destroyCancellationToken);
            _processing = CreateLinkedTokenSource(destroyCancellationToken);

            while (_processing.IsCancellationRequested is not true)
            {
                await RoutineAsync(_graphic.materialForRendering, WaveFactor, WaveDuration, _processing.Token);

                await UniTask.Delay(IterationDelay, Realtime, FixedUpdate, _processing.Token)
                    .SuppressCancellationThrow();
            }

            return;

            static async UniTask RoutineAsync
            (
                Material material,
                (double Start, double End) waveFactor,
                TimeSpan waveDuration,
                CancellationToken cancellation = default
            ) {
                if (cancellation.IsCancellationRequested) return;

                material.SetFloat(DistanceFromCenter, (float) waveFactor.Start);

                var elapsedTime = TimeSpan.Zero;

                while (elapsedTime < waveDuration && cancellation.IsCancellationRequested is false)
                {
                    var normalizedTime = elapsedTime / waveDuration;
                    var wave = (float) math.lerp(waveFactor.Start, waveFactor.End, normalizedTime);
                    material.SetFloat(DistanceFromCenter, wave);

                    await UniTask.Yield(FixedUpdate, cancellation)
                        .SuppressCancellationThrow();

                    elapsedTime += TimeSpan.FromSeconds(Time.fixedDeltaTime);
                }
            }
        }

        private void OnDisable() => _processing.Cancel();
    }
}
