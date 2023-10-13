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
        private readonly ICurrentTimeProvider _currentTime;

        public PreparationJob
        (
            TMP_CharacterInfo characterInfo,
            Vector3[] vertices,
            Tween tween,
            ICurrentTimeProvider currentTime
        ) {
            _characterInfo = characterInfo;
            _vertices = vertices;
            _tween = tween;
            _currentTime = currentTime;
        }

        public UniTask<Vector3[]> ExecuteAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(_vertices);
            if (_characterInfo.isVisible is false) return UniTask.FromResult(_vertices);

            for (var vertex = 0; vertex < 4; ++vertex)
            {
                var current = _characterInfo.vertexIndex + vertex;
                var origin = _vertices[current];
                _vertices[current] += _tween.Invoke(origin, _currentTime.Value);
            }

            return UniTask.FromResult(_vertices);
        }
    }
}
