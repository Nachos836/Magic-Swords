using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.TextAnimator.TextPlaying.PlayingJobs
{
    using Effect;

    internal readonly struct PreparationJob
    {
        private readonly TMP_CharacterInfo _characterInfo;
        private readonly Vector3[] _vertices;
        private readonly Tween _tween;

        public PreparationJob(TMP_CharacterInfo characterInfo, Vector3[] vertices, Tween tween)
        {
            _characterInfo = characterInfo;
            _vertices = vertices;
            _tween = tween;
        }

        public UniTask<Vector3[]> ExecuteAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(_vertices);
            if (_characterInfo.isVisible is false) return UniTask.FromResult(_vertices);

            for (var j = 0; j < 4; ++j)
            {
                var origin = _vertices[_characterInfo.vertexIndex + j];
                _vertices[_characterInfo.vertexIndex + j] += _tween.Invoke(origin);
            }

            return UniTask.FromResult(_vertices);
        }
    }
}
