using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MagicSwords.Features.MainMenu.Shaders
{
    internal sealed class ShockWaveEffect : MonoBehaviour
    {
        private static readonly WaitForFixedUpdate FixedUpdate = new ();
        private static readonly int DistanceFromCenter = Shader.PropertyToID("_DistanceFromCenter");
        private static readonly int RingSpawnPosition = Shader.PropertyToID("_RingSpawnPosition");

        [SerializeField] private int _delay = 5;
        [SerializeField] private float _duration = 0.75f;
        [SerializeField] private Material _material;
        [SerializeField] private RectTransform _startPosition;

        private void Start()
        {
            var position = (_startPosition.anchorMax + _startPosition.anchorMin) / 2;

            _material.SetVector(RingSpawnPosition, position);

            EffectLoop(destroyCancellationToken).Forget();

            async UniTaskVoid EffectLoop(CancellationToken cancellation = default)
            {
                while (destroyCancellationToken.IsCancellationRequested is false)
                {
                    await UniTask.Delay(_delay * 1000, DelayType.Realtime, PlayerLoopTiming.FixedUpdate, cancellation)
                        .SuppressCancellationThrow();

                    await Routine((-0.1f, 1f))
                        .WithCancellation(cancellation)
                        .SuppressCancellationThrow();

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellation)
                        .SuppressCancellationThrow();
                }
            }
        }

        private IEnumerator Routine((float start, float end) position)
        {
            _material.SetFloat(DistanceFromCenter, position.start);

            var elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                elapsedTime += Time.fixedDeltaTime;

                var amount = Mathf.Lerp(position.start, position.end, elapsedTime / _duration);
                _material.SetFloat(DistanceFromCenter, amount);

                yield return FixedUpdate;
            }
        }
    }
}
