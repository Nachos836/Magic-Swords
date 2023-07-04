using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features.Miscellaneous
{
    internal interface ISceneLoader
    {
        UniTask LoadAlongsideAsync(int buildIndex, CancellationToken cancellation = default);
        UniTask TransferToAsync(int buildIndex, CancellationToken cancellation = default);
    }

    internal sealed class SceneLoader : ISceneLoader
    {
        async UniTask ISceneLoader.LoadAlongsideAsync(int buildIndex, CancellationToken cancellation)
        {
            await SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive)
                .WithCancellation(cancellation)
                .SuppressCancellationThrow();
        }

        async UniTask ISceneLoader.TransferToAsync(int buildIndex, CancellationToken cancellation)
        {
            await SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single)
                .WithCancellation(cancellation)
                .SuppressCancellationThrow();
        }
    }
}
