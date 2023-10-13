using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace MagicSwords.Features.Text
{
    using Generic.Functional;
    using AnimatedRichText;
    using AnimatedRichText.Playing;

    [CreateAssetMenu(menuName = "Novel Framework/Text/Create Text/Sequenced Text")]
    internal sealed class SequencedText : ScriptableObject, IText
    {
        [SerializeReference] private RichText[] _pieces;

        private void OnValidate() => _pieces ??= Array.Empty<RichText>();

        AsyncLazy<AsyncResult> IText.PresentAsync(Player player, CancellationToken cancellation)
        {
            return _pieces.ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .Select(text => ((IText)text).PresentAsync(player, cancellation))
                .AggregateAwaitWithCancellationAsync(accumulator: static async (first, second, token) =>
                {
                    if (token.IsCancellationRequested) return UniTask.FromResult(AsyncResult.Cancel).ToAsyncLazy();

                    await first;

                    if (token.IsCancellationRequested) return UniTask.FromResult(AsyncResult.Cancel).ToAsyncLazy();

                    return second;
                }, cancellation)
                .ContinueWith(static async last => await last)
                .ToAsyncLazy();
        }
    }

    internal interface IText
    {
        AsyncLazy<AsyncResult> PresentAsync(Player player, CancellationToken cancellation = default);
    }
}
