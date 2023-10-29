using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing.Jobs
{
    using Animating;
    using TimeProvider;

    internal readonly struct PreparationJob
    {
        private readonly TMP_CharacterInfo _characterInfo;
        private readonly Vector3[] _vertices;
        private readonly Tween _tween;
        private readonly IFixedCurrentTimeProvider _currentTime;

        public PreparationJob
        (
            TMP_CharacterInfo characterInfo,
            Vector3[] vertices,
            Tween tween,
            IFixedCurrentTimeProvider currentTime
        ) {
            _characterInfo = characterInfo;
            _vertices = vertices;
            _tween = tween;
            _currentTime = currentTime;
        }

        public UniTask ExecuteAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;
            if (_characterInfo.isVisible is false) return UniTask.CompletedTask;

            for (var vertex = 0; vertex < 4; ++vertex)
            {
                var current = _characterInfo.vertexIndex + vertex;
                var origin = _vertices[current];
                _vertices[current] += _tween.Invoke(origin, _currentTime.Value);
            }

            return UniTask.CompletedTask;
        }
    }
}
