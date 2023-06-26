using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MagicSwords.Features
{
    internal interface ISceneSwitcher
    {
        UniTask SwitchAsync(int buildIndex, CancellationToken cancellation = default);
    }

    internal sealed class SceneSwitcher : ISceneSwitcher
    {
        async UniTask ISceneSwitcher.SwitchAsync(int buildIndex, CancellationToken cancellation)
        {
            await SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive)
                .WithCancellation(cancellation)
                .SuppressCancellationThrow();
        }
    }
}