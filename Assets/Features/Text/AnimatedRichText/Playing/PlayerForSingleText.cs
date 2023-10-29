using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;
using Random = UnityEngine.Random;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Animating;
    using Jobs;
    using TimeProvider;
    using Generic.Functional;
    using Generic.Functional.Outcome;

    internal sealed class PlayerForSingleText : ITextPlayer, ITextDisplayingListener
    {
        private readonly TMP_Text _field;
        private readonly IText _text;
        private readonly IFixedCurrentTimeProvider _currentTime;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly Channel<Unit> _displayStream;

        public PlayerForSingleText
        (
            TMP_Text field,
            IText text,
            IFixedCurrentTimeProvider currentTime,
            PlayerLoopTiming yieldPoint = PlayerLoopTiming.Update
        ) {
            _field = field;
            _text = text;
            _currentTime = currentTime;
            _yieldPoint = yieldPoint;
            _displayStream = Channel.CreateSingleConsumerUnbounded<Unit>();
        }

        IUniTaskAsyncEnumerable<Unit> ITextDisplayingListener.DisplayingStreamAsync(CancellationToken cancellation)
        {
            return _displayStream.Reader.ReadAllAsync(cancellation);
        }

        async UniTask<AsyncResult<AnimationDisposingHandler>> PlayAsync(CancellationToken cancellation)
        {
            var writer = _displayStream.Writer;

            long renderFlag = default;

            var (plainText, tweens) = await _text.ProvidePresetAsync(cancellation);

            _field.renderMode = TextRenderFlags.DontRender;
            _field.text = plainText;

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

                    writer.TryWrite(Unit.Instance);
                }
            }

            return writer.TryComplete()
                ? AsyncResult<AnimationDisposingHandler>.FromResult(AnimationDisposingHandler.None)
                : AsyncResult<AnimationDisposingHandler>.Error;
        }

        private static IUniTaskAsyncEnumerable<IRevealAsyncEffect> BuildRevealStreamAsync
        (
            TMP_Text field,
            TimeSpan delay,
            CancellationToken cancellation = default
        ) {
            return Repeat((field, delay), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) => (IRevealAsyncEffect) new FadeInEffect(income.field, income.delay, current))
                .Prepend(new DisplayingActivationEffect(field));
        }

        private static IUniTaskAsyncEnumerable<IIdleAsyncEffect> BuildIdleStreamAsync
        (
            TMP_Text field,
            IFixedCurrentTimeProvider timeProvider,
            IEffect effectPreset,
            TimeSpan delay,
            CancellationToken cancellation = default
        ) {
            return Repeat((field, timeProvider, effectPreset, delay), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) => new WobbleEffect(income.field, income.timeProvider, income.effectPreset, income.delay, current));
        }

        private IUniTaskAsyncEnumerable<IDissolveAsyncEffect> BuildDissolveStreamAsync
        (
            TMP_Text field,
            TimeSpan delay,
            CancellationToken cancellation
        )
        {
            return Repeat((field, delay), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) => (IDissolveAsyncEffect) new FadeOutEffect(income.field, income.delay, current));
        }

        async UniTask<AsyncResult<AnimationDisposingHandler>> ITextPlayer.PlayAsync(CancellationToken cancellation)
        {
            Time.fixedDeltaTime = 1.0f / 1024f;
            var delay = TimeSpan.FromTicks(1);

            var preset = await _text.ProvidePresetAsync(cancellation);
            var text = preset.PlainText;
            IEffect effect = new Animating.Wobble.WobbleEffect()
                .Configure(0.01f, 0.5f);

            _field.renderMode = TextRenderFlags.DontRender;
            _field.enabled = false;
            _field.text = text;
            _field.color = Color.clear;
            _field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

            var appearanceEffectsStream = BuildRevealStreamAsync(_field, delay, cancellation);
            var idleEffectsStream = BuildIdleStreamAsync(_field, _currentTime, effect, delay, cancellation);
            var disappearanceEffectsStream = BuildDissolveStreamAsync(_field, delay, cancellation);

            var appearanceEnumerator = appearanceEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);
            var idleEffectsEnumerator = idleEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);
            var disappearanceEnumerator = disappearanceEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);

            await appearanceEnumerator.MoveNextAsync();
            await appearanceEnumerator.Current.ApplyAsync(cancellation);

            var idleThenDestroyHandler = AnimationDisposingHandler.BuildAsync(disappearanceEnumerator, cancellation);
            var idleCancellation = idleThenDestroyHandler.IdleCancellation;

            while ((await UniTask.WhenAll(tasks: new[]
            {
                appearanceEnumerator.MoveNextAsync().SuppressCancellationThrow(),
                idleEffectsEnumerator.MoveNextAsync().SuppressCancellationThrow()

            })).Aggregate(static (first, second) => (first.IsCanceled & second.IsCanceled, first.Result & second.Result))
                   is { Result: true } tuple)
            {
                if (tuple.IsCanceled) return AsyncResult<AnimationDisposingHandler>.Cancel;

                await appearanceEnumerator.Current.ApplyAsync(cancellation);
                idleEffectsEnumerator.Current.ApplyAsync(idleCancellation);
            }

            await appearanceEnumerator.DisposeAsync()
                .SuppressCancellationThrow();

            return AsyncResult<AnimationDisposingHandler>.FromResult(idleThenDestroyHandler);
        }

        private static IUniTaskAsyncEnumerable<PreparationJob> PrepareTextPiecesAsync
        (
            Tween[] tweens,
            TMP_TextInfo textInfo,
            IFixedCurrentTimeProvider currentTime,
            CancellationToken cancellation = default
        ) {
            return textInfo.characterInfo
                .ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .TakeWhile(static character => character is not { character: '\0' })
                .Select((characterInfo, index) => new PreparationJob
                (
                    characterInfo,
                    textInfo.meshInfo[characterInfo.materialReferenceIndex].vertices,
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

    internal readonly struct AnimationDisposingHandler
    {
        private readonly IUniTaskAsyncEnumerator<IDissolveAsyncEffect> _disappearanceEnumerator;
        private readonly CancellationTokenSource _idleEffectsLifetime;

        private AnimationDisposingHandler(IUniTaskAsyncEnumerator<IDissolveAsyncEffect> disappearanceEnumerator, CancellationTokenSource idleEffectsLifetime)
        {
            _disappearanceEnumerator = disappearanceEnumerator;
            _idleEffectsLifetime = idleEffectsLifetime;
        }

        public CancellationToken IdleCancellation => _idleEffectsLifetime.Token;

        public static AnimationDisposingHandler None { get; } = new
        (
            Empty<IDissolveAsyncEffect>().GetAsyncEnumerator(),
            CancellationTokenSource.CreateLinkedTokenSource(token1: CancellationToken.None, token2: CancellationToken.None)
        );

        public static AnimationDisposingHandler BuildAsync
        (
            IUniTaskAsyncEnumerator<IDissolveAsyncEffect> disappearanceEnumerator,
            CancellationToken cancellation
        ) {
            return new AnimationDisposingHandler
            (
                disappearanceEnumerator,
                CancellationTokenSource.CreateLinkedTokenSource(token1: cancellation, token2: CancellationToken.None)
            );
        }

        public async UniTask DisposeAsync(CancellationToken cancellation = default)
        {
            while (cancellation.IsCancellationRequested is false
            && (await _disappearanceEnumerator.MoveNextAsync().SuppressCancellationThrow()).Result)
            {
                if (cancellation.IsCancellationRequested) return;

                await _disappearanceEnumerator.Current.ApplyAsync(cancellation);
            }

            _idleEffectsLifetime.Cancel();
            _idleEffectsLifetime.Dispose();
            await _disappearanceEnumerator.DisposeAsync()
                .SuppressCancellationThrow();
        }
    }

    internal interface IRevealAsyncEffect
    {
        UniTask ApplyAsync(CancellationToken cancellation = default);
    }

    internal interface IIdleAsyncEffect
    {
        UniTaskVoid ApplyAsync(CancellationToken cancellation = default);
    }

    internal interface IDissolveAsyncEffect
    {
        UniTask ApplyAsync(CancellationToken cancellation = default);
    }

    internal sealed class DisplayingActivationEffect : IRevealAsyncEffect
    {
        private readonly TMP_Text _field;

        public DisplayingActivationEffect(TMP_Text field) => _field = field;

        UniTask IRevealAsyncEffect.ApplyAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            _field.renderMode = TextRenderFlags.Render;

            _field.enabled = true;

            return UniTask.CompletedTask;
        }
    }

    internal sealed class WobbleEffect : IIdleAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly IFixedCurrentTimeProvider _timeProvider;
        private readonly IEffect _wobblePreset;
        private readonly TimeSpan _delay;
        private readonly int _current;

        public WobbleEffect(TMP_Text field, IFixedCurrentTimeProvider timeProvider, IEffect wobblePreset, TimeSpan delay, int current)
        {
            _field = field;
            _timeProvider = timeProvider;
            _wobblePreset = wobblePreset;
            _delay = delay;
            _current = current;
        }

        async UniTaskVoid IIdleAsyncEffect.ApplyAsync(CancellationToken cancellation)
        {
            var characterInfo = _field.textInfo.characterInfo[_current];
            var meshIndex = characterInfo.materialReferenceIndex;
            var vertices = _field.textInfo.meshInfo[meshIndex].vertices;

            var preparationJob = new PreparationJob(characterInfo, vertices, _wobblePreset.Tween, _timeProvider);
            var showingJob = new ShowingJob(_field, meshIndex);

            while (cancellation.IsCancellationRequested is false)
            {
                await preparationJob.ExecuteAsync(cancellation);

                await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, PlayerLoopTiming.FixedUpdate, cancellation)
                    .SuppressCancellationThrow();

                await showingJob.ExecuteAsync(cancellation);
            }
        }
    }

    internal sealed class NoneEffect : IRevealAsyncEffect
    {
        UniTask IRevealAsyncEffect.ApplyAsync(CancellationToken _) => UniTask.CompletedTask;
    }

    internal sealed class FadeInEffect : IRevealAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly TimeSpan _delay;
        private readonly int _current;

        public FadeInEffect(TMP_Text field, TimeSpan delay, int current)
        {
            _field = field;
            _delay = delay;
            _current = current;
        }

        async UniTask IRevealAsyncEffect.ApplyAsync(CancellationToken cancellation)
        {
            var meshIndex = _field.textInfo.characterInfo[_current].materialReferenceIndex;
            var newVertexColors = _field.textInfo.meshInfo[meshIndex].colors32;

            var vertexIndex = _field.textInfo.characterInfo[_current].vertexIndex;

            var c1 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
            newVertexColors[vertexIndex + 0] = c1;
            newVertexColors[vertexIndex + 1] = c1;
            newVertexColors[vertexIndex + 2] = c1;
            newVertexColors[vertexIndex + 3] = c1;

            _field.textInfo.meshInfo[meshIndex].colors32 = newVertexColors;

            if (await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, PlayerLoopTiming.FixedUpdate, cancellation)
                .SuppressCancellationThrow()) return;

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

    internal sealed class FadeOutEffect : IDissolveAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly TimeSpan _delay;
        private readonly int _current;

        public FadeOutEffect(TMP_Text field, TimeSpan delay, int current)
        {
            _field = field;
            _delay = delay;
            _current = current;
        }

        async UniTask IDissolveAsyncEffect.ApplyAsync(CancellationToken cancellation)
        {
            var meshIndex = _field.textInfo.characterInfo[_current].materialReferenceIndex;
            var newVertexColors = _field.textInfo.meshInfo[meshIndex].colors32;

            var vertexIndex = _field.textInfo.characterInfo[_current].vertexIndex;

            Color32 c1 = Color.clear;
            newVertexColors[vertexIndex + 0] = c1;
            newVertexColors[vertexIndex + 1] = c1;
            newVertexColors[vertexIndex + 2] = c1;
            newVertexColors[vertexIndex + 3] = c1;

            _field.textInfo.meshInfo[meshIndex].colors32 = newVertexColors;

            if (await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, PlayerLoopTiming.FixedUpdate, cancellation)
                .SuppressCancellationThrow()) return;

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
