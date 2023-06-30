using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.Command
{
    public interface IAsyncCommand
    {
        UniTask ExecuteAsync(CancellationToken cancellation = default);
    }
}