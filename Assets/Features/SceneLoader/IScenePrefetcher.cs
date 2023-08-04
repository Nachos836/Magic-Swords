using System.Threading;

namespace MagicSwords.Features.SceneLoader
{
    public interface IScenePrefetcher
    {
        void PrefetchAsync(CancellationToken cancellation = default);
    }
}
