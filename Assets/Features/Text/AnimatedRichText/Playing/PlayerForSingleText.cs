using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using VContainer;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;
using static Cysharp.Threading.Tasks.UniTaskStatus;

using Random = UnityEngine.Random;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Animating;
    using Jobs;
    using Input;
    using TimeProvider;
    using Generic.Functional;

    internal sealed class PlayerForSingleText : ITextPlayer
    {
        private static readonly UniTask<(bool IsCanceled, bool Result)>[] IterationTasks = new UniTask<(bool IsCanceled, bool Result)>[2];

        private readonly TMP_Text _field;
        private readonly IText _text;
        private readonly IFixedCurrentTimeProvider _currentTime;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly IInputFor<ReadingSkip> _inputForSkip;

        public PlayerForSingleText
        (
            TMP_Text field,
            IText text,
            IFixedCurrentTimeProvider currentTime,
            PlayerLoopTiming yieldPoint,
            IInputFor<ReadingSkip> inputForSkip
        ) {
            _field = field;
            _text = text;
            _currentTime = currentTime;
            _yieldPoint = yieldPoint;
            _inputForSkip = inputForSkip;
        }

        async UniTask<AsyncResult<DissolveAnimationsHandler>> ITextPlayer.PlayAsync(CancellationToken cancellation)
        {
            var delay = TimeSpan.FromTicks(1);
            var preset = await _text.ProvidePresetAsync(cancellation);
            var text = preset.PlainText;
            var effect = new Animating.Wobble.WobbleEffect()
                .Configure(0.01f, 0.5f);

            var interruption = new UniTaskCompletionSource();

            using var _ = _inputForSkip.Subscribe(started: _ => interruption.TrySetResult());

            await BootstrapAsync(_field, text, cancellation);

            var revealEffectsStream = BuildRevealStreamAsync(_field, delay, _yieldPoint, cancellation);
            var idleEffectsStream = BuildIdleStreamAsync(_field, _currentTime, effect, delay, _yieldPoint, cancellation);
            var dissolveEffectsStream = BuildDissolveStreamAsync(_field, delay, _yieldPoint, cancellation);

            var revealEnumerator = revealEffectsStream.TakeUntilCanceled(cancellation)
                .TakeUntil(interruption.Task)
                .GetAsyncEnumerator(cancellation);
            var idleEffectsEnumerator = idleEffectsStream.TakeUntilCanceled(cancellation)
                .TakeUntil(interruption.Task)
                .GetAsyncEnumerator(cancellation);

            await revealEnumerator.MoveNextAsync();
            await revealEnumerator.Current.ApplyAsync(cancellation);

            AsyncRichResult iteration;
            var processedCount = 0;
            do
            {
                iteration = await KeepProcessingAsync(revealEnumerator, idleEffectsEnumerator, cancellation);
                if (iteration.IsCancellation) return AsyncResult<DissolveAnimationsHandler>.Cancel;
                if (iteration.IsError) return iteration.AsResult().Attach(DissolveAnimationsHandler.None);

                await revealEnumerator.Current.ApplyAsync(cancellation);
                idleEffectsEnumerator.Current.ApplyAsync(cancellation);

                ++processedCount;
            }
            while (iteration.IsSuccessful);

            await idleEffectsEnumerator.DisposeAsync()
                .AttachExternalCancellation(cancellation)
                .SuppressCancellationThrow();
            await revealEnumerator.DisposeAsync()
                .AttachExternalCancellation(cancellation)
                .SuppressCancellationThrow();

            return AsyncResult<DissolveAnimationsHandler>.FromResult
            (
                DissolveAnimationsHandler.Build
                (
                    dissolveEffectsStream,
                    processedCount,
                    _inputForSkip
                )
            );

            static async UniTask<AsyncRichResult> KeepProcessingAsync
            (
                IUniTaskAsyncEnumerator<IRevealAsyncEffect> appearanceEnumerator,
                IUniTaskAsyncEnumerator<IIdleAsyncEffect> idleEffectsEnumerator,
                CancellationToken token = default
            ) {
                try
                {
                    IterationTasks[0] = appearanceEnumerator.MoveNextAsync()
                        .AttachExternalCancellation(token)
                        .SuppressCancellationThrow();
                    IterationTasks[1] = idleEffectsEnumerator.MoveNextAsync()
                        .AttachExternalCancellation(token)
                        .SuppressCancellationThrow();
                }
                catch (Exception exception)
                {
                    return AsyncRichResult.FromException(exception);
                }

                var iteration = await UniTask.WhenAll(IterationTasks)
                    .AttachExternalCancellation(token)
                    .SuppressCancellationThrow();

                if (iteration.IsCanceled) return AsyncRichResult.Cancel;

                var combine = iteration.Result.Aggregate(static (first, second) =>
                {
                    return (first.IsCanceled & second.IsCanceled, first.Result | second.Result);
                });

                if (combine.IsCanceled)
                {
                    return AsyncRichResult.Cancel;
                }
                else if (combine.Result)
                {
                    return AsyncRichResult.Success;
                }
                else
                {
                    return AsyncRichResult.Failure;
                }
            }

            static UniTask BootstrapAsync(TMP_Text field, string text, CancellationToken cancellation = default)
            {
                if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

                field.renderMode = TextRenderFlags.DontRender;
                field.enabled = false;
                field.text = text;
                field.color = Color.clear;
                field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

                return UniTask.CompletedTask;
            }
        }

        async UniTask<AsyncResult<DissolveAnimationsHandler>> PlayAsync(CancellationToken cancellation)
        {
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
                }
            }

            return AsyncResult<DissolveAnimationsHandler>.FromResult(DissolveAnimationsHandler.None);
        }

        private static IUniTaskAsyncEnumerable<IRevealAsyncEffect> BuildRevealStreamAsync
        (
            TMP_Text field,
            TimeSpan delay,
            PlayerLoopTiming yieldPoint,
            CancellationToken cancellation = default
        ) {
            return Repeat((field, delay, yieldPoint), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) => (IRevealAsyncEffect) new FadeInEffect(income.field, income.delay, income.yieldPoint, current))
                .Prepend(new DisplayingActivationEffect(field));
        }

        private static IUniTaskAsyncEnumerable<IIdleAsyncEffect> BuildIdleStreamAsync
        (
            TMP_Text field,
            IFixedCurrentTimeProvider timeProvider,
            IEffect effectPreset,
            TimeSpan delay,
            PlayerLoopTiming yieldPoint,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Empty<IIdleAsyncEffect>();

            return Create<IIdleAsyncEffect>(create: async (writer, token) =>
            {
                var effects = Repeat((field, timeProvider, effectPreset, delay, yieldPoint), field.textInfo.characterInfo.Length)
                    .TakeUntilCanceled(token)
                    .Select(static (income, current) =>
                    {
                        var (field, timeProvider, effectPreset, delay, yieldPoint) = income;

                        if (current is 5) return (IIdleAsyncEffect)new IIdleAsyncEffect.ApplyStrategyAlongside
                        (
                            first: new WobbleEffect(field, timeProvider, effectPreset, delay, yieldPoint, current),
                            second: new ConsoleTriggerEffect()
                        );

                        return new WobbleEffect(field, timeProvider, effectPreset, delay, yieldPoint, current);
                    });

                await foreach (var effect in effects.WithCancellation(token))
                {
                    if (token.IsCancellationRequested) return;

                    await writer.YieldAsync(effect);
                }

                while (token.IsCancellationRequested is not true)
                {
                    await writer.YieldAsync(NoneIdleEffectAsync.Instance);
                }
            });
        }

        private IUniTaskAsyncEnumerable<IDissolveAsyncEffect> BuildDissolveStreamAsync
        (
            TMP_Text field,
            TimeSpan delay,
            PlayerLoopTiming yieldPoint,
            CancellationToken cancellation = default
        ) {
            return Repeat((field, delay, yieldPoint), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) => (IDissolveAsyncEffect) new FadeOutEffect(income.field, income.delay, income.yieldPoint, current));
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

    [InjectIgnore]
    internal readonly struct DissolveAnimationsHandler
    {
        private readonly IUniTaskAsyncEnumerable<IDissolveAsyncEffect> _disappearanceStream;
        private readonly int _totalEffects;
        private readonly IInputFor<ReadingSkip> _inputForSkip;
        private readonly UniTaskCompletionSource _idleEffectsLifetime;

        private DissolveAnimationsHandler
        (
            IUniTaskAsyncEnumerable<IDissolveAsyncEffect> disappearanceStream,
            int totalEffects,
            IInputFor<ReadingSkip> inputForSkip,
            UniTaskCompletionSource idleEffectsLifetime
        ) {
            _disappearanceStream = disappearanceStream;
            _totalEffects = totalEffects;
            _inputForSkip = inputForSkip;
            _idleEffectsLifetime = idleEffectsLifetime;
        }

        public static DissolveAnimationsHandler None { get; } = new
        (
            disappearanceStream: Empty<IDissolveAsyncEffect>(),
            totalEffects: 0,
            inputForSkip: default!,
            idleEffectsLifetime: new UniTaskCompletionSource()
        );

        public static DissolveAnimationsHandler Build
        (
            IUniTaskAsyncEnumerable<IDissolveAsyncEffect> disappearanceStream,
            int total,
            IInputFor<ReadingSkip> inputForSkip
        ) {
            return new DissolveAnimationsHandler
            (
                disappearanceStream,
                total,
                inputForSkip,
                new UniTaskCompletionSource()
            );
        }

        public async UniTask SoftCancellationAsync(CancellationToken hardCancellation = default)
        {
            if (_totalEffects is 0) return;

            var streamCandidate = await _disappearanceStream.TakeUntilCanceled(hardCancellation)
                .Take(_totalEffects)
                .Select(static (effect, current) => (effect, current))
                .ToArrayAsync(hardCancellation)
                .SuppressCancellationThrow();

            if (streamCandidate.IsCanceled) return;

            var effects = streamCandidate.Result.ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(hardCancellation)
                .TakeUntil(_idleEffectsLifetime.Task);

            var lastEffectIndex = 0;
            var idleEffectsLifetime = _idleEffectsLifetime;
            using (_inputForSkip.Subscribe(started: SoftCancelOnSkipAction))
            {
                await foreach (var (effect, current) in effects.WithCancellation(hardCancellation))
                {
                    if (hardCancellation.IsCancellationRequested) return;

                    await effect.ApplyAsync(hardCancellation);
                    lastEffectIndex = current;
                }
            }

            if (_idleEffectsLifetime.Task.Status is Succeeded) await UniTask.WhenAll
            (
                streamCandidate.Result.TakeLast(count: _totalEffects - lastEffectIndex)
                    .Select(tuple => tuple.effect.ApplyAsync(hardCancellation))
            );

            return;

            void SoftCancelOnSkipAction(StartedContext _)
            {
                if (hardCancellation.IsCancellationRequested) idleEffectsLifetime.TrySetCanceled(hardCancellation);

                idleEffectsLifetime.TrySetResult();
            }
        }
    }

    internal interface IRevealAsyncEffect
    {
        UniTask ApplyAsync(CancellationToken cancellation = default);
    }

    internal interface IIdleAsyncEffect
    {
        UniTaskVoid ApplyAsync(CancellationToken cancellation = default);

        internal sealed class ApplyStrategyAlongside : IIdleAsyncEffect
        {
            private readonly IIdleAsyncEffect _first;
            private readonly IIdleAsyncEffect _second;

            public ApplyStrategyAlongside(IIdleAsyncEffect first, IIdleAsyncEffect second)
            {
                _first = first;
                _second = second;
            }

            UniTaskVoid IIdleAsyncEffect.ApplyAsync(CancellationToken cancellation)
            {
                _first.ApplyAsync(cancellation).Forget();
                return _second.ApplyAsync(cancellation);
            }
        }
    }

    internal interface IDissolveAsyncEffect
    {
        UniTask ApplyAsync(CancellationToken cancellation = default);
    }

    [InjectIgnore]
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

    [InjectIgnore]
    internal sealed class NoneIdleEffectAsync : IIdleAsyncEffect
    {
        public static readonly IIdleAsyncEffect Instance = new NoneIdleEffectAsync();
        private static readonly UniTaskVoid Nothing = default;

        private NoneIdleEffectAsync() {}

        UniTaskVoid IIdleAsyncEffect.ApplyAsync(CancellationToken _) => Nothing;
    }

    [InjectIgnore]
    internal sealed class WobbleEffect : IIdleAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly IFixedCurrentTimeProvider _timeProvider;
        private readonly IEffect _wobblePreset;
        private readonly TimeSpan _delay;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly int _current;

        public WobbleEffect(TMP_Text field, IFixedCurrentTimeProvider timeProvider, IEffect wobblePreset, TimeSpan delay, PlayerLoopTiming yieldPoint, int current)
        {
            _field = field;
            _timeProvider = timeProvider;
            _wobblePreset = wobblePreset;
            _delay = delay;
            _yieldPoint = yieldPoint;
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

                await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation)
                    .SuppressCancellationThrow();

                await showingJob.ExecuteAsync(cancellation);
            }
        }
    }

    [InjectIgnore]
    internal sealed class ConsoleTriggerEffect : IIdleAsyncEffect
    {
        UniTaskVoid IIdleAsyncEffect.ApplyAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return default;

            Debug.Log("Triggered");

            return default;
        }
    }

    [InjectIgnore]
    internal sealed class NoneEffect : IRevealAsyncEffect
    {
        UniTask IRevealAsyncEffect.ApplyAsync(CancellationToken _) => UniTask.CompletedTask;
    }

    [InjectIgnore]
    internal sealed class FadeInEffect : IRevealAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly TimeSpan _delay;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly int _current;

        public FadeInEffect(TMP_Text field, TimeSpan delay, PlayerLoopTiming yieldPoint, int current)
        {
            _field = field;
            _delay = delay;
            _yieldPoint = yieldPoint;
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

            if (await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

    [InjectIgnore]
    internal sealed class FadeOutEffect : IDissolveAsyncEffect
    {
        private readonly TMP_Text _field;
        private readonly TimeSpan _delay;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly int _current;

        public FadeOutEffect(TMP_Text field, TimeSpan delay, PlayerLoopTiming yieldPoint, int current)
        {
            _field = field;
            _delay = delay;
            _yieldPoint = yieldPoint;
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

            if (await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
