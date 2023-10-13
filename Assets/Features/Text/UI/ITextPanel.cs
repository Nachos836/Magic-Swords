using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.UI
{
    using Generic.Functional;

    public interface ITextPanel
    {
        UniTask<AsyncResult<IDisposable>> LoadAsync(CancellationToken cancellation = default);
    }
}
