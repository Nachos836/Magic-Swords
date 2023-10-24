using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Animating;
    using Jobs;
    using TimeProvider;
    using Generic.Functional;

    internal sealed class Player
    {
        private readonly TMP_Text _field;
        private readonly IText _text;
        private readonly ICurrentTimeProvider _currentTime;
        private readonly PlayerLoopTiming _yieldPoint;

        public Player
        (
            TMP_Text field,
            IText text,
            ICurrentTimeProvider currentTime,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Update
        ) {
            _field = field;
            _text = text;
            _currentTime = currentTime;
            _yieldPoint = yieldPoint;
        }

        public async UniTask<AsyncResult> PlayAsync(CancellationToken cancellation = default)
        {
            long renderFlag = default;

            var preset = await _text.ProvidePresetAsync(cancellation);

            _field.renderMode = TextRenderFlags.DontRender;
            _field.text = preset.PlainText;
            var tweens = preset.Tweens;

            await foreach (var _ in EveryUpdate(_yieldPoint).TakeUntilCanceled(cancellation))
            {
                _field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                var textInfo = _field.textInfo;

                await foreach (var preparation in PrepareTextPiecesAsync(tweens, textInfo, _currentTime, cancellation))
                {
                    await preparation.ExecuteAsync(cancellation);
                }

                if (Interlocked.Read(ref renderFlag) is 0)
                {
                    Interlocked.Add(ref renderFlag, 1);

                    _field.renderMode = TextRenderFlags.Render;
                }

                await foreach (var showing in ShowTextPiecesAsync(textInfo, _field, cancellation))
                {
                    await showing.ExecuteAsync(cancellation);
                }
            }

            return AsyncResult.Success;
        }

        private static IUniTaskAsyncEnumerable<PreparationJob> PrepareTextPiecesAsync
        (
            Tween[] tweens,
            TMP_TextInfo textInfo,
            ICurrentTimeProvider currentTime,
            CancellationToken cancellation = default
        ) {
            return textInfo.characterInfo
                .ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .TakeWhile(static character => character is not { character: '\0' })
                .Select((character, index) => new PreparationJob
                (
                    character,
                    textInfo.meshInfo[character.materialReferenceIndex].vertices,
                    tweens[index],
                    currentTime
                ));
        }

        private static IUniTaskAsyncEnumerable<ShowingJob> ShowTextPiecesAsync
        (
            TMP_TextInfo textInfo,
            TMP_Text field,
            CancellationToken cancellation = default
        ) {
            return textInfo.meshInfo
                .ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .Select((_, current) => new ShowingJob(field, current));
        }
    }
}
