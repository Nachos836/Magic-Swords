using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.TextAnimator.TextPlaying
{
    using PlayingJobs;
    using Effect;

    internal sealed class TextPlayer
    {
        private readonly TMP_Text _field;
        private readonly PlayerLoopTiming _yieldPoint;

        public TextPlayer(TMP_Text field, PlayerLoopTiming yieldPoint)
        {
            _field = field;
            _yieldPoint = yieldPoint;
        }

        public async UniTask PlayAsync
        (
            string text,
            IQueryable<Tween> tweens,
            CancellationToken cancellation = default
        ) {
            long renderFlag = default;

            _field.renderMode = TextRenderFlags.DontRender;
            _field.text = text;

            await foreach (var _ in EveryUpdate(_yieldPoint).TakeUntilCanceled(cancellation))
            {
                _field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

                // await UniTask.SwitchToTaskPool();

                await foreach (var preparation in PrepareTextPiecesAsync(tweens, cancellation))
                {
                    await preparation.ExecuteAsync(cancellation);
                }

                // await UniTask.SwitchToMainThread(cancellation);

                if (Interlocked.Read(ref renderFlag) is 0)
                {
                    Interlocked.Add(ref renderFlag, 1);

                    _field.renderMode = TextRenderFlags.Render;
                }

                await foreach (var showing in ShowTextPiecesAsync(cancellation))
                {
                    await showing.ExecuteAsync(cancellation);
                }
            }
        }

        private async IAsyncEnumerable<PreparationJob> PrepareTextPiecesAsync
        (
            IQueryable<Tween> tweens,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            var textInfo = _field.textInfo;

            await foreach (var characterInfo in textInfo.characterInfo
                               .AsParallel()
                               .ToUniTaskAsyncEnumerable()
                               .TakeUntilCanceled(cancellation)
            ) {
                var current = characterInfo.materialReferenceIndex;

                yield return new PreparationJob
                (
                    characterInfo,
                    textInfo.meshInfo[current].vertices,
                    tweens.ElementAt(current)
                );
            }
        }

        private async IAsyncEnumerable<ShowingJob> ShowTextPiecesAsync
        (
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            var textInfo = _field.textInfo;

            await foreach (var i in textInfo.meshInfo
                               .Select((_, i) => i)
                               .AsParallel()
                               .ToUniTaskAsyncEnumerable()
                               .TakeUntilCanceled(cancellation)
            ) {
                yield return new ShowingJob(_field, i);
            }
        }
    }
}
