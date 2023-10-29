using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Generic.Functional;
    using Generic.Functional.Outcome;

    internal interface ITextPlayer
    {
        UniTask<AsyncResult<AnimationDisposingHandler>> PlayAsync(CancellationToken cancellation = default);
    }

    internal interface ITextDisplayingListener
    {
        IUniTaskAsyncEnumerable<Unit> DisplayingStreamAsync(CancellationToken cancellation = default);
    }
}
