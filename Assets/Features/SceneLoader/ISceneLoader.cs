using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.SceneLoader
{
    public interface ISceneLoader
    {
        UniTask LoadAlongsideAsync(int buildIndex, CancellationToken cancellation = default);
        UniTask TransferToAsync(int buildIndex, CancellationToken cancellation = default);
    }
}
