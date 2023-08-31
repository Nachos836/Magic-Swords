using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.AnimatedRichText.Playing
{
    using Animating;
    using Jobs;
    using TimeProvider;

    internal readonly struct Player
    {
        private readonly TMP_Text _field;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly ICurrentTimeProvider _currentTime;

        public Player
        (
            TMP_Text field,
            PlayerLoopTiming yieldPoint,
            ICurrentTimeProvider currentTime
        ) {
            _field = field;
            _yieldPoint = yieldPoint;
            _currentTime = currentTime;
        }

        public async UniTask PlayAsync
        (
            string text,
            Tween[] tweens,
            CancellationToken cancellation = default
        ) {
            long renderFlag = default;

            _field.renderMode = TextRenderFlags.DontRender;
            _field.text = text;

            await foreach (var _ in EveryUpdate(_yieldPoint).TakeUntilCanceled(cancellation))
            {
                _field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                var textInfo = _field.textInfo;

                await foreach (var preparation in PrepareTextPiecesAsync(tweens, textInfo, cancellation))
                {
                    await preparation.ExecuteAsync(cancellation);
                }

                if (Interlocked.Read(ref renderFlag) is 0)
                {
                    Interlocked.Add(ref renderFlag, 1);

                    _field.renderMode = TextRenderFlags.Render;
                }

                await foreach (var showing in ShowTextPiecesAsync(textInfo, cancellation))
                {
                    await showing.ExecuteAsync(cancellation);
                }
            }
        }

        private async IAsyncEnumerable<PreparationJob> PrepareTextPiecesAsync
        (
            Tween[] tweens,
            TMP_TextInfo textInfo,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            await foreach (var (character, current) in textInfo.characterInfo
                               .ToUniTaskAsyncEnumerable()
                               .TakeWhile(character => character is not { character: '\0' })
                               .Select((character, index) => (character, index))
                               .TakeUntilCanceled(cancellation)
            ) {
                var materialIndex = character.materialReferenceIndex;

                yield return new PreparationJob
                (
                    character,
                    textInfo.meshInfo[materialIndex].vertices,
                    tweens[current],
                    _currentTime
                );
            }
        }

        private async IAsyncEnumerable<ShowingJob> ShowTextPiecesAsync
        (
            TMP_TextInfo textInfo,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            await foreach (var current in textInfo.meshInfo
                               .Select((_, index) => index)
                               .ToUniTaskAsyncEnumerable()
                               .TakeUntilCanceled(cancellation)
            ) {
                yield return new ShowingJob(_field, current);
            }
        }
    }
}
