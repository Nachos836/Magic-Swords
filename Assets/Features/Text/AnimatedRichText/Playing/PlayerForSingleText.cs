﻿using System;
using System.Collections.Generic;
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
                .Configure(strength: 0.01f, amplitude: 0.5f);
            var interruptionTimeout = TimeSpan.FromSeconds(2);

            await BootstrapAsync(_field, text, cancellation);

            var revealEffectsStream = BuildRevealStreamAsync(_field, delay, _yieldPoint, cancellation);
            var idleEffectsStream = BuildIdleStreamAsync(_field, _currentTime, effect, delay, _yieldPoint, cancellation);

            await using var revealEnumerator = revealEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);
            await using var idleEffectsEnumerator = idleEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);

            AsyncRichResult iteration;
            var totalSymbols = _field.textInfo.characterInfo.Length;
            do
            {
                var (iterationIndex, nextOneIterated, allHasIterated) = await UniTask.WhenAny
                (
                    task1: ProcessNextAsync(revealEnumerator, idleEffectsEnumerator, cancellation),
                    task2: ProcessAllAtOnceAsync(revealEnumerator, idleEffectsEnumerator, _inputForSkip, totalSymbols, cancellation)
                );

                iteration = iterationIndex is 0
                    ? await nextOneIterated.RunAsync(async token => await ApplyNextEffectsAsync(revealEnumerator, idleEffectsEnumerator, token), cancellation)
                    : allHasIterated;

                if (iteration.IsCancellation) return AsyncResult<DissolveAnimationsHandler>.Cancel;
                if (iteration.IsError) return iteration.AsResult().Attach(DissolveAnimationsHandler.None);

                continue;

                static async UniTask ApplyNextEffectsAsync
                (
                    IUniTaskAsyncEnumerator<IRevealAsyncEffect> revealEnumerator,
                    IUniTaskAsyncEnumerator<IIdleAsyncEffect> idleEffectsEnumerator,
                    CancellationToken token = default
                ) {
                    await revealEnumerator.Current.ApplyAsync(token);
                    idleEffectsEnumerator.Current.ApplyAsync(token);
                }
            }
            while (iteration.IsSuccessful);

            var (interruptionIndex, actionInterrupted, timeoutInterrupted) = await UniTask.WhenAny
            (
                task1: InputActionInterruptionAsync(_inputForSkip, idleEffectsEnumerator, _yieldPoint, cancellation),
                task2: TimeoutInterruptionAsync(interruptionTimeout, _yieldPoint, cancellation)
            );
            var interruption = interruptionIndex is 0 ? actionInterrupted : timeoutInterrupted;

            return interruption.Attach(DissolveAnimationsHandler.Build
            (
                BuildDissolveStreamAsync(_field, delay, _yieldPoint, cancellation),
                _field.textInfo.characterInfo.Length,
                _inputForSkip
            ));

            static async UniTask<AsyncResult> InputActionInterruptionAsync
            (
                IInputFor<ReadingSkip> inputForSkip,
                IUniTaskAsyncEnumerator<IIdleAsyncEffect> idleEffectsEnumerator,
                PlayerLoopTiming yieldPoint,
                CancellationToken token = default
            ) {
                var inputActionInterrupted = false;
                using var _ = inputForSkip.Subscribe(started: _ => inputActionInterrupted = true);

                while (inputActionInterrupted is not true)
                {
                    if (token.IsCancellationRequested) return AsyncResult.Cancel;

                    if (await idleEffectsEnumerator.MoveNextAsync())
                    {
                        idleEffectsEnumerator.Current.ApplyAsync(token);
                    }

                    if (await UniTask.Yield(yieldPoint, token)
                        .SuppressCancellationThrow()) return AsyncResult.Cancel;
                }

                return AsyncResult.Success;
            }

            static async UniTask<AsyncResult> TimeoutInterruptionAsync
            (
                TimeSpan interruptionTimeout,
                PlayerLoopTiming yieldPoint,
                CancellationToken token = default
            ) {
                if (await UniTask.Delay(interruptionTimeout, DelayType.UnscaledDeltaTime, yieldPoint, token, cancelImmediately: true)
                    .SuppressCancellationThrow()) return AsyncResult.Cancel;

                return AsyncResult.Success;
            }

            static async UniTask<AsyncRichResult> ProcessNextAsync
            (
                IUniTaskAsyncEnumerator<IRevealAsyncEffect> revealEnumerator,
                IUniTaskAsyncEnumerator<IIdleAsyncEffect> idleEffectsEnumerator,
                CancellationToken token = default
            ) {
                if (token.IsCancellationRequested) return AsyncRichResult.Cancel;

                var (reveal, idle) = await (revealEnumerator.MoveNextAsync(), idleEffectsEnumerator.MoveNextAsync());

                return reveal & idle ? AsyncRichResult.Success : AsyncRichResult.Failure;
            }

            static async UniTask<AsyncRichResult> ProcessAllAtOnceAsync
            (
                IUniTaskAsyncEnumerator<IRevealAsyncEffect> revealEnumerator,
                IUniTaskAsyncEnumerator<IIdleAsyncEffect> idleEffectsEnumerator,
                IInputFor<ReadingSkip> inputForSkip,
                int totalCapacity,
                CancellationToken token = default
            ) {
                var interruption = new UniTaskCompletionSource<AsyncRichResult>();
                using var _ = inputForSkip.Subscribe
                (
                    started: _ => interruption.TrySetResult(AsyncRichResult.Failure)
                );

                if ((await interruption.Task.AttachExternalCancellation(token)
                    .SuppressCancellationThrow()).IsCanceled) return AsyncRichResult.Cancel;

                var effectsTasks = new List<UniTask>(totalCapacity);
                do
                {
                    if (token.IsCancellationRequested) return AsyncRichResult.Cancel;

                    var (reveal, idle) = await (revealEnumerator.MoveNextAsync(), idleEffectsEnumerator.MoveNextAsync());
                    if ((reveal & idle) is false) break;

                    effectsTasks.Add(revealEnumerator.Current.ApplyAsync(token));
                    idleEffectsEnumerator.Current.ApplyAsync(token);

                } while (true);

                await UniTask.WhenAll(effectsTasks)
                    .AttachExternalCancellation(token)
                    .SuppressCancellationThrow();

                return token.IsCancellationRequested ? AsyncRichResult.Cancel : AsyncRichResult.Failure;
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

                if (token.IsCancellationRequested is not true)
                {
                    await writer.YieldAsync(NoneIdleEffectAsync.Instance);
                }

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

        private static IUniTaskAsyncEnumerable<IDissolveAsyncEffect> BuildDissolveStreamAsync
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
    }

    [InjectIgnore]
    internal readonly struct DissolveAnimationsHandler
    {
        private readonly IUniTaskAsyncEnumerable<IDissolveAsyncEffect> _disappearanceStream;
        private readonly int _totalEffects;
        private readonly IInputFor<ReadingSkip> _inputForSkip;
        private readonly UniTaskCompletionSource _idleEffectsLifetime;

        public static DissolveAnimationsHandler None { get; } = new
        (
            disappearanceStream: Empty<IDissolveAsyncEffect>(),
            totalEffects: 0,
            inputForSkip: default!,
            idleEffectsLifetime: new UniTaskCompletionSource()
        );

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

            if (_idleEffectsLifetime.UnsafeGetStatus() is Succeeded) await UniTask.WhenAll
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

        [InjectIgnore]
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

                await showingJob.ExecuteAsync(cancellation);

                await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation, cancelImmediately: true)
                    .SuppressCancellationThrow();
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

            var c1 = new Color32((byte) Random.Range(0, 255), (byte) Random.Range(0, 255), (byte) Random.Range(0, 255), 255);
            newVertexColors[vertexIndex + 0] = c1;
            newVertexColors[vertexIndex + 1] = c1;
            newVertexColors[vertexIndex + 2] = c1;
            newVertexColors[vertexIndex + 3] = c1;

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation, cancelImmediately: true)
                .SuppressCancellationThrow();
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

            _field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            await UniTask.Delay(_delay, DelayType.UnscaledDeltaTime, _yieldPoint, cancellation, cancelImmediately: true)
                .SuppressCancellationThrow();
        }
    }
}
