using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.SceneLoader
{
    using Generic.Functional;

    public interface ISceneSwitcher
    {
        UniTask<AsyncResult> SwitchAsync(CancellationToken cancellation = default);
    }
}
