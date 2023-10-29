using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing.Jobs
{
    internal readonly struct ShowingJob
    {
        private readonly TMP_Text _field;
        private readonly int _meshIndex;

        public ShowingJob(TMP_Text field, int meshIndex)
        {
            _field = field;
            _meshIndex = meshIndex;
        }

        public UniTask ExecuteAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            var meshInfo = _field.textInfo.meshInfo[_meshIndex];
            meshInfo.mesh.vertices = meshInfo.vertices;

            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            _field.UpdateGeometry(meshInfo.mesh, _meshIndex);

            return UniTask.CompletedTask;
        }
    }
}
