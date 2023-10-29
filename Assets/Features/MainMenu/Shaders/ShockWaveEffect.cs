using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

using static Cysharp.Threading.Tasks.DelayType;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

namespace MagicSwords.Features.MainMenu.Shaders
{
    internal sealed class ShockWaveEffect : MonoBehaviour
    {
        private static readonly int DistanceFromCenter = Shader.PropertyToID("_DistanceFromCenter");
        private static readonly int RingSpawnPosition = Shader.PropertyToID("_RingSpawnPosition");

        [SerializeField] private int _delay = 5;
        [SerializeField] private float _duration = 0.75f;
        [SerializeField] private Material _material;
        [SerializeField] private RectTransform _startPosition;

        [UsedImplicitly] // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            var position = (_startPosition.anchorMax + _startPosition.anchorMin) / 2;

            _material.SetVector(RingSpawnPosition, position);

            await EffectLoop(destroyCancellationToken);

            return;

            async UniTask EffectLoop(CancellationToken cancellation = default)
            {
                while (destroyCancellationToken.IsCancellationRequested is false)
                {
                    await UniTask.Delay(_delay * 1000, Realtime, FixedUpdate, cancellation)
                        .SuppressCancellationThrow();

                    await RoutineAsync((-0.1f, 1f), cancellation);

                    await UniTask.Yield(FixedUpdate, cancellation)
                        .SuppressCancellationThrow();
                }
            }
        }

        private async UniTask RoutineAsync((float start, float end) position, CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return;

            _material.SetFloat(DistanceFromCenter, position.start);

            var elapsedTime = 0f;

            while (elapsedTime < _duration && cancellation.IsCancellationRequested is false)
            {
                elapsedTime += Time.fixedDeltaTime;

                var amount = Mathf.Lerp(position.start, position.end, elapsedTime / _duration);
                _material.SetFloat(DistanceFromCenter, amount);

                await UniTask.Yield(FixedUpdate, cancellation)
                    .SuppressCancellationThrow();
            }
        }
    }
}
