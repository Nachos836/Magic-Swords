using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.SceneLoader
{
    using Generic.Functional;

    public interface ISceneLoader
    {
        UniTask<AsyncResult> LoadAsync(CancellationToken cancellation = default);
    }
}
