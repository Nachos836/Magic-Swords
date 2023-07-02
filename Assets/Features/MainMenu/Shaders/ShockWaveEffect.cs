using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MagicSwords.Features.MainMenu.Shaders
{
    internal sealed class ShockWaveEffect : MonoBehaviour
    {
        private static readonly int DistanceFromCenter = Shader.PropertyToID("_DistanceFromCenter");

        [SerializeField] private float _duration = 0.75f;
        [SerializeField] private Material _material;

        private async void Start()
        {
            await RoutineAsync(-0.1f, 1f, destroyCancellationToken);
        }

        // private async void FixedUpdate()
        // {
        //     await RoutineAsync(-0.1f, 1f, destroyCancellationToken);
        // }

        private async UniTask RoutineAsync(float startPosition, float endPosition, CancellationToken cancellation = default)
        {
            while (destroyCancellationToken.IsCancellationRequested is false)
            {
                _material.SetFloat(DistanceFromCenter, startPosition);

                var elapsedTime = 0f;

                while (elapsedTime < _duration || cancellation.IsCancellationRequested is false)
                {
                    elapsedTime += Time.fixedDeltaTime;

                    var amount = Mathf.Lerp(startPosition, endPosition, (elapsedTime / _duration));
                    _material.SetFloat(DistanceFromCenter, amount);

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellation)
                        .SuppressCancellationThrow();
                }
                
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellation)
                    .SuppressCancellationThrow();
            }
        }
    }
}
