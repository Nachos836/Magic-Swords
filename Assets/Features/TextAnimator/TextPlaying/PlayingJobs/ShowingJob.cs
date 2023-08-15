using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace MagicSwords.Features.TextAnimator.TextPlaying.PlayingJobs
{
    internal readonly struct ShowingJob
    {
        private readonly TMP_Text _field;
        private readonly int _current;

        public ShowingJob(TMP_Text field, int current)
        {
            _field = field;
            _current = current;
        }

        public UniTask ExecuteAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            var meshInfo = _field.textInfo.meshInfo[_current];
            meshInfo.mesh.vertices = _field.textInfo.meshInfo[_current].vertices;

            _field.UpdateGeometry(meshInfo.mesh, _current);

            return UniTask.CompletedTask;
        }
    }
}
