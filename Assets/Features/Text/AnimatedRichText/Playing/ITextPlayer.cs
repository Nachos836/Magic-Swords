using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Generic.Functional;

    internal interface ITextPlayer
    {
        UniTask<AsyncResult<DissolveAnimationsHandler>> PlayAsync(CancellationToken cancellation = default);
    }
}
