using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using VContainer;
using ZBase.Foundation.PolymorphicStructs;

using static Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable;
using static Cysharp.Threading.Tasks.PlayerLoopTiming;

using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace MagicSwords.Features.Text.AnimatedRichText.Playing
{
    using Input;
    using TimeProvider;
    using Generic.Functional;
    using SingleProducerSingleConsumer;

    using static Generic.ExtendDotNet.CancellationTokenSourceExtensions;

    [InjectIgnore]
    internal readonly struct Timer
    {
        public const float CompleteNormalizedTime = 1.0f;

        public static readonly Timer None = new (deltaTime: TimeSpan.Zero, duration: TimeSpan.Zero);

        private readonly TimeSpan _deltaTime;
        private readonly TimeSpan _duration;

        public Timer(TimeSpan deltaTime, TimeSpan duration)
        {
            _deltaTime = deltaTime;
            _duration = duration;
        }

        public UniTaskCancelableAsyncEnumerable<float> StartAsync(CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return Empty<float>().WithCancellation(cancellation);
            if (_duration == TimeSpan.Zero) return Return(CompleteNormalizedTime).WithCancellation(cancellation);

            var delta = _deltaTime;
            var startTime = TimeSpan.Zero;
            var duration = _duration;

            return Create<float>(async (writer, token) =>
            {
                while (startTime <= duration && token.IsCancellationRequested is not true)
                {
                    await writer.YieldAsync((float) (startTime / duration));

                    startTime += delta;
                }

                if (startTime != duration && token.IsCancellationRequested is not true)
                {
                    await writer.YieldAsync(CompleteNormalizedTime);
                }
            }).WithCancellation(cancellation);
        }
    }

    internal sealed class PlayerForSingleText : ITextPlayer
    {
        private readonly TMP_Text _field;
        private readonly IText _text;
        private readonly IFixedCurrentTimeProvider _currentTime;
        private readonly IFixedDeltaTimeProvider _deltaTime;
        private readonly PlayerLoopTiming _presentationPoint;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly IInputFor<ReadingSkip> _inputForSkip;

        public PlayerForSingleText
        (
            TMP_Text field,
            IText text,
            IFixedCurrentTimeProvider currentTime,
            IFixedDeltaTimeProvider deltaTime,
            PlayerLoopTiming presentationPoint,
            PlayerLoopTiming initializationPoint,
            IInputFor<ReadingSkip> inputForSkip
        ) {
            _field = field;
            _text = text;
            _currentTime = currentTime;
            _deltaTime = deltaTime;
            _presentationPoint = presentationPoint;
            _initializationPoint = initializationPoint;
            _inputForSkip = inputForSkip;
        }

        async UniTask<AsyncResult<DissolveAnimationsHandler>> ITextPlayer.PlayAsync(CancellationToken cancellation)
        {
            await UniTask.Yield(_initializationPoint, cancellation, cancelImmediately: true)
                .SuppressCancellationThrow();

            var duration = TimeSpan.FromTicks(1);
            var deltaTime = TimeSpan.FromSeconds(_deltaTime.Value);
            var preset = await _text.ProvidePresetAsync(cancellation);
            var text = preset.PlainText;
            var interruptionTimeout = TimeSpan.FromSeconds(1);

            var preparationStream = BuildPreparationStreamAsync(_field, text, cancellation);
            await using var preparationEnumerator = preparationStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);

            if (await preparationEnumerator.MoveNextAsync()) await preparationEnumerator.Current.Execute(cancellation);

            var revealEffectsStream = BuildRevealStreamAsync(_field, deltaTime, duration, cancellation);
            var idleEffectsStream = BuildIdleStreamAsync(_field, cancellation);

            await using var revealEnumerator = revealEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);
            await using var idleEffectsEnumerator = idleEffectsStream.TakeUntilCanceled(cancellation)
                .GetAsyncEnumerator(cancellation);

            var idleLasting = new CancellationTokenDisposable(cancellation);
            cancellation.RegisterWithoutCaptureExecutionContext(() => idleLasting.Dispose());

            var idleEffects = new Queue<IdleAsyncEffect>((uint)_field.textInfo.characterInfo.Length);
            IdleEffectsLoopAsync(idleEffects, _currentTime, _presentationPoint, idleLasting.Token)
                .Forget();

            await using var _ = FieldUpdateHandler.BuildAsync(_field, _presentationPoint, idleLasting.Token);

            if (await preparationEnumerator.MoveNextAsync()) await preparationEnumerator.Current.Execute(cancellation);

            await UniTask.Yield(_presentationPoint, cancellation, cancelImmediately: true)
                .SuppressCancellationThrow();

            AsyncRichResult iteration;
            do
            {
                var (iterationIndex, nextOneIterated, allHasIterated) = await UniTask.WhenAny
                (
                    task1: ProcessNextAsync(revealEnumerator, idleEffectsEnumerator, cancellation),
                    task2: ProcessAllAtOnceAsync(revealEnumerator, idleEffectsEnumerator, idleEffects, _inputForSkip, _presentationPoint, _field, cancellation)
                );

                iteration = iterationIndex is 0
                    ? await nextOneIterated.RunAsync(async token => await ApplyNextEffectsAsync(revealEnumerator, idleEffectsEnumerator, idleEffects, _presentationPoint, token), cancellation)
                    : allHasIterated;

                if (iteration.IsCancellation) return AsyncResult<DissolveAnimationsHandler>.Cancel;
                if (iteration.IsError) return iteration.AsResult().Attach(DissolveAnimationsHandler.None);

                continue;

                static async UniTask ApplyNextEffectsAsync
                (
                    IUniTaskAsyncEnumerator<(RevealAsyncEffect Effect, Timer Timer)> revealEnumerator,
                    IUniTaskAsyncEnumerator<IdleAsyncEffect> idleEffectsEnumerator,
                    IProducerQueue<IdleAsyncEffect> idleEffectsWriter,
                    PlayerLoopTiming yieldPoint,
                    CancellationToken token = default
                ) {
                    var reveal = revealEnumerator.Current;

                    await using var timer = reveal.Timer.StartAsync(token)
                        .GetAsyncEnumerator();

                    if (await timer.MoveNextAsync() && token.IsCancellationRequested is not true)
                    {
                        reveal.Effect.ApplyAsync(timer.Current, token);
                        await idleEffectsWriter.EnqueueAsync(idleEffectsEnumerator.Current, yieldPoint, token);
                    }

                    while (await timer.MoveNextAsync() && token.IsCancellationRequested is not true)
                    {
                        await UniTask.Yield(yieldPoint, token, cancelImmediately: true)
                            .SuppressCancellationThrow();

                        reveal.Effect.ApplyAsync(timer.Current, token);
                    }
                }
            }
            while (iteration.IsSuccessful);

            var (interruptionIndex, actionInterrupted, timeoutInterrupted) = await UniTask.WhenAny
            (
                task1: InputActionInterruptionAsync(_inputForSkip, _presentationPoint, cancellation),
                task2: TimeoutInterruptionAsync(interruptionTimeout, _presentationPoint, cancellation)
            );
            var interruption = interruptionIndex is 0 ? actionInterrupted : timeoutInterrupted;

            return interruption.Attach(new DissolveAnimationsHandler
            (
                _field,
                BuildDissolveStreamAsync(_field, deltaTime, duration, cancellation),
                idleLasting,
                _inputForSkip,
                _field.textInfo.characterInfo.Length,
                _presentationPoint
            ));

            static async UniTaskVoid IdleEffectsLoopAsync
            (
                IConsumerQueue<IdleAsyncEffect> effects,
                IFixedCurrentTimeProvider currentTime,
                PlayerLoopTiming yieldPoint,
                CancellationToken token = default
            ) {
                if (token.IsCancellationRequested) return;

                while (token.IsCancellationRequested is not true)
                {
                    if (effects.TryDequeue(out var current) is not true)
                    {
                        await UniTask.Yield(yieldPoint, token, cancelImmediately: true)
                            .SuppressCancellationThrow();

                        continue;
                    }

                    EffectRunnerAsync(current, currentTime, yieldPoint, token)
                        .Forget();

                    continue;

                    static async UniTaskVoid EffectRunnerAsync
                    (
                        IdleAsyncEffect effect,
                        IFixedCurrentTimeProvider currentTime,
                        PlayerLoopTiming yieldPoint,
                        CancellationToken token = default
                    ) {
                        while (token.IsCancellationRequested is not true)
                        {
                            await UniTask.Yield(yieldPoint, token, cancelImmediately: true)
                                .SuppressCancellationThrow();

                            effect.ApplyAsync(currentTime.Value, token);
                        }
                    }
                }
            }

            static async UniTask<AsyncResult> InputActionInterruptionAsync
            (
                IInputFor<ReadingSkip> inputForSkip,
                PlayerLoopTiming yieldPoint,
                CancellationToken token = default
            ) {
                var inputActionInterrupted = false;
                using var _ = inputForSkip.Subscribe(started: _ =>
                {
                    inputActionInterrupted = true;
                });

                if (await UniTask.WaitUntil(() => inputActionInterrupted, yieldPoint, token, cancelImmediately: true)
                    .SuppressCancellationThrow()) return AsyncResult.Cancel;

                return AsyncResult.Success;
            }

            static async UniTask<AsyncResult> TimeoutInterruptionAsync
            (
                TimeSpan interruptionTimeout,
                PlayerLoopTiming yieldPoint,
                CancellationToken token = default
            ) {
                if (await UniTask.Delay(interruptionTimeout, DelayType.Realtime, yieldPoint, token, cancelImmediately: true)
                    .SuppressCancellationThrow()) return AsyncResult.Cancel;

                return AsyncResult.Success;
            }

            static async UniTask<AsyncRichResult> ProcessNextAsync
            (
                IUniTaskAsyncEnumerator<(RevealAsyncEffect Effect, Timer Timer)> revealEnumerator,
                IUniTaskAsyncEnumerator<IdleAsyncEffect> idleEffectsEnumerator,
                CancellationToken token = default
            ) {
                if (token.IsCancellationRequested) return AsyncRichResult.Cancel;

                var (reveal, idle) = await (revealEnumerator.MoveNextAsync(), idleEffectsEnumerator.MoveNextAsync());

                return reveal & idle ? AsyncRichResult.Success : AsyncRichResult.Failure;
            }

            static async UniTask<AsyncRichResult> ProcessAllAtOnceAsync
            (
                IUniTaskAsyncEnumerator<(RevealAsyncEffect Effect, Timer Timer)> revealEnumerator,
                IUniTaskAsyncEnumerator<IdleAsyncEffect> idleEffectsEnumerator,
                IProducerQueue<IdleAsyncEffect> idleEffectsPool,
                IInputFor<ReadingSkip> inputForSkip,
                PlayerLoopTiming yieldPoint,
                TMP_Text field,
                CancellationToken token = default
            ) {
                var interruption = new UniTaskCompletionSource<AsyncRichResult>();
                using var _ = inputForSkip.Subscribe
                (
                    started: _ => interruption.TrySetResult(AsyncRichResult.Failure)
                );

                if ((await interruption.Task.AttachExternalCancellation(token)
                    .SuppressCancellationThrow()).IsCanceled) return AsyncRichResult.Cancel;

                await using (FieldUpdateHandler.BuildAsync(field, yieldPoint, token)) do
                {
                    if (token.IsCancellationRequested) return AsyncRichResult.Cancel;

                    var (reveal, idle) = await (revealEnumerator.MoveNextAsync(), idleEffectsEnumerator.MoveNextAsync());
                    if ((reveal & idle) is false) break;

                    revealEnumerator.Current.Effect.ApplyAsync(t: Timer.CompleteNormalizedTime, token);
                    await idleEffectsPool.EnqueueAsync(idleEffectsEnumerator.Current, yieldPoint, token);

                } while (true);

                return token.IsCancellationRequested ? AsyncRichResult.Cancel : AsyncRichResult.Failure;
            }
        }

        private static IUniTaskAsyncEnumerable<PreparationJob> BuildPreparationStreamAsync
        (
            TMP_Text field,
            string text,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Empty<PreparationJob>();

            return Create<PreparationJob>(async (writer, token) =>
            {
                if (token.IsCancellationRequested) return;

                await writer.YieldAsync(new DisplayingBootstrapJob(field, text));

                if (token.IsCancellationRequested) return;

                await writer.YieldAsync(new DisplayingActivationJob(field));
            });
        }

        private static IUniTaskAsyncEnumerable<(RevealAsyncEffect Effect, Timer Timer)> BuildRevealStreamAsync
        (
            TMP_Text field,
            TimeSpan deltaTime,
            TimeSpan effectDuration,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Empty<(RevealAsyncEffect, Timer)>();

            return Repeat((field, delta: deltaTime, effectDuration), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) =>
                {
                    var (field, deltaTime, effectDuration) = income;
                    var characterInfo = field.textInfo.characterInfo[current];

                    if (characterInfo.isVisible is not true) return (RevealAsyncEffect.None, Timer.None);

                    return
                    (
                        (RevealAsyncEffect) new FadeInEffect(field, current),
                        new Timer(deltaTime, effectDuration)
                    );
                });
        }

        private static IUniTaskAsyncEnumerable<IdleAsyncEffect> BuildIdleStreamAsync
        (
            TMP_Text field,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Empty<IdleAsyncEffect>();

            return Create<IdleAsyncEffect>(create: async (writer, token) =>
            {
                var effects = Repeat(field, field.textInfo.characterInfo.Length)
                    .TakeUntilCanceled(token)
                    .Select(static (field, current) =>
                    {
                        var characterInfo = field.textInfo.characterInfo[current];
                        if (characterInfo.isVisible is not true) return IdleAsyncEffect.None;

                        return (IdleAsyncEffect) new WobbleEffect(field, current);
                    });

                await foreach (var effect in effects.WithCancellation(token))
                {
                    if (token.IsCancellationRequested) return;

                    await writer.YieldAsync(effect);
                }

                while (token.IsCancellationRequested is not true)
                {
                    await writer.YieldAsync(NoneIdleAsyncEffect.Instance);
                }
            });
        }

        private static IUniTaskAsyncEnumerable<(DissolveAsyncEffect Effect, Timer Timer)> BuildDissolveStreamAsync
        (
            TMP_Text field,
            TimeSpan deltaTime,
            TimeSpan effectDuration,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Empty<(DissolveAsyncEffect, Timer)>();

            return Repeat((field, delta: deltaTime, effectDuration), field.textInfo.characterInfo.Length)
                .TakeUntilCanceled(cancellation)
                .Select(static (income, current) =>
                {
                    var (field, deltaTime, effectDuration) = income;
                    var characterInfo = field.textInfo.characterInfo[current];

                    if (characterInfo.isVisible is not true) return (DissolveAsyncEffect.None, Timer.None);

                    return
                    (
                        (DissolveAsyncEffect) new FadeOutEffect(field, current),
                        new Timer(deltaTime, effectDuration)
                    );
                });
        }
    }

    [InjectIgnore]
    internal readonly struct DissolveAnimationsHandler
    {
        private readonly TMP_Text _field;
        private readonly IUniTaskAsyncEnumerable<(DissolveAsyncEffect Effect, Timer Timer)> _disappearanceStream;
        private readonly CancellationTokenDisposable _idleLasting;
        private readonly IInputFor<ReadingSkip> _inputForSkip;
        private readonly int _totalEffects;
        private readonly PlayerLoopTiming _yieldPoint;
        private readonly UniTaskCompletionSource _idleEffectsLifetime;

        public static DissolveAnimationsHandler None { get; } = new
        (
            field: default!,
            disappearanceStream: Empty<(DissolveAsyncEffect, Timer)>(),
            idleLasting: new CancellationTokenDisposable(),
            inputForSkip: default!,
            totalEffects: 0,
            yieldPoint: Initialization
        );

        internal DissolveAnimationsHandler
        (
            TMP_Text field,
            IUniTaskAsyncEnumerable<(DissolveAsyncEffect, Timer)> disappearanceStream,
            CancellationTokenDisposable idleLasting,
            IInputFor<ReadingSkip> inputForSkip,
            int totalEffects,
            PlayerLoopTiming yieldPoint
        ) {
            _field = field;
            _disappearanceStream = disappearanceStream;
            _idleLasting = idleLasting;
            _inputForSkip = inputForSkip;
            _totalEffects = totalEffects;
            _yieldPoint = yieldPoint;
            _idleEffectsLifetime = new UniTaskCompletionSource();
        }

        public async UniTask SoftCancellationAsync(CancellationToken hardCancellation = default)
        {
            using var __ = _idleLasting;
            if (_totalEffects is 0) return;

            var idleEffectsLifetime = _idleEffectsLifetime;
            hardCancellation.RegisterWithoutCaptureExecutionContext(() => idleEffectsLifetime.TrySetCanceled());

            var (isCanceled, result) = await _disappearanceStream.TakeUntilCanceled(hardCancellation)
                .Take(_totalEffects)
                .Select(static (effect, current) => (effect, current))
                .ToArrayAsync(hardCancellation)
                .SuppressCancellationThrow();

            if (isCanceled) return;

            var effects = result.ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(hardCancellation)
                .TakeUntil(_idleEffectsLifetime.Task);

            var lastEffectIndex = 0;

            await using var _ = FieldUpdateHandler.BuildAsync(_field, _yieldPoint, hardCancellation);

            using (_inputForSkip.Subscribe(target: _idleEffectsLifetime, started: SoftCancelOnSkipAction))
            {
                await UniTask.Yield(_yieldPoint, hardCancellation, cancelImmediately: true)
                    .SuppressCancellationThrow();

                await foreach (var (candidate, current) in effects.WithCancellation(hardCancellation))
                {
                    if (hardCancellation.IsCancellationRequested) return;

                    var (effect, timer) = candidate;

                    await foreach (var tick in timer.StartAsync(hardCancellation))
                    {
                        await UniTask.Yield(_yieldPoint, hardCancellation, cancelImmediately: true)
                            .SuppressCancellationThrow();

                        effect.ApplyAsync(tick, hardCancellation);
                        lastEffectIndex = current;
                    }
                }
            }

            await UniTask.Yield(_yieldPoint, hardCancellation, cancelImmediately: true)
                .SuppressCancellationThrow();

            var leftovers = result.ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(hardCancellation)
                .TakeLast(count: _totalEffects - lastEffectIndex)
                .Select(static income => income.effect.Effect);

            await foreach (var effect in leftovers.WithCancellation(hardCancellation))
            {
                effect.ApplyAsync(Timer.CompleteNormalizedTime, hardCancellation);
            }

            await UniTask.Yield(_yieldPoint, hardCancellation, cancelImmediately: true)
                .SuppressCancellationThrow();

            return;

            static void SoftCancelOnSkipAction(UniTaskCompletionSource idleEffectsLifetime, StartedContext _)
            {
                idleEffectsLifetime.TrySetResult();
            }
        }
    }

    [PolymorphicStructInterface]
    public interface IPreparationJob
    {
        UniTask Execute(CancellationToken cancellation = default);
    }

    [PolymorphicStructInterface]
    public interface IEffectAsyncConsumption
    {
        UniTask ConsumeAsync(CancellationToken cancellation = default);
    }

    [PolymorphicStructInterface]
    public interface IRevealAsyncEffect
    {
        void ApplyAsync(float t, CancellationToken cancellation = default);
    }

    public partial struct RevealAsyncEffect
    {
        public static readonly RevealAsyncEffect None = new NoneRevealAsyncEffect();
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct NoneRevealAsyncEffect : IRevealAsyncEffect
    {
        void IRevealAsyncEffect.ApplyAsync(float _, CancellationToken __) { }
    }

    [PolymorphicStructInterface]
    public interface IIdleAsyncEffect
    {
        void ApplyAsync(float time, CancellationToken cancellation = default);
    }

    public partial struct IdleAsyncEffect
    {
        public static readonly IdleAsyncEffect None = new NoneIdleAsyncEffect();
    }

    [PolymorphicStructInterface]
    public interface IDissolveAsyncEffect
    {
        void ApplyAsync(float normalizedTime, CancellationToken cancellation = default);
    }

    public partial struct DissolveAsyncEffect
    {
        public static readonly DissolveAsyncEffect None = new NoneDissolveAsyncEffect();
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct NoneDissolveAsyncEffect : IDissolveAsyncEffect
    {
        void IDissolveAsyncEffect.ApplyAsync(float _, CancellationToken __) { }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct DisplayingBootstrapJob : IPreparationJob
    {
        private readonly TMP_Text _field;
        private readonly string _text;

        UniTask IPreparationJob.Execute(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            _field.renderMode = TextRenderFlags.DontRender;
            _field.enabled = false;
            _field.text = _text;
            _field.color = new Color(r: 1, g: 1, b: 1, a: 0);
            _field.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

            return UniTask.CompletedTask;
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct DisplayingActivationJob : IPreparationJob
    {
        private readonly TMP_Text _field;

        UniTask IPreparationJob.Execute(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return UniTask.CompletedTask;

            _field.renderMode = TextRenderFlags.Render;
            _field.enabled = true;
            _field.ForceMeshUpdate();

            return UniTask.CompletedTask;
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct FadeInEffect : IRevealAsyncEffect
    {
        private readonly TMP_Text _applicationField;
        private readonly int _current;

        void IRevealAsyncEffect.ApplyAsync(float normalizedTime, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return;

            var characterInfo = _applicationField.textInfo.characterInfo[_current];
            var meshIndex = characterInfo.materialReferenceIndex;
            var meshInfo = _applicationField.textInfo.meshInfo[meshIndex];
            var newVertexColors = meshInfo.colors32;
            var modificationIndex = characterInfo.vertexIndex;

            const byte alpha = 0;
            const byte visible = 255;

            newVertexColors[modificationIndex + 0].a = Lerp(from: alpha, to: visible, normalizedTime);
            newVertexColors[modificationIndex + 1].a = Lerp(from: alpha, to: visible, normalizedTime);
            newVertexColors[modificationIndex + 2].a = Lerp(from: alpha, to: visible, normalizedTime);
            newVertexColors[modificationIndex + 3].a = Lerp(from: alpha, to: visible, normalizedTime);

            return;

            static byte Lerp(byte from, byte to, float normalizedTime)
            {
                var value = (byte) (from + normalizedTime * (to - from));

                return value;
            }
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct NoneIdleAsyncEffect : IIdleAsyncEffect
    {
        internal static readonly IdleAsyncEffect Instance = new NoneIdleAsyncEffect();

        void IIdleAsyncEffect.ApplyAsync(float _, CancellationToken __) { }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct WobbleEffect : IIdleAsyncEffect
    {
        private readonly TMP_Text _updateField;
        private readonly int _current;

        void IIdleAsyncEffect.ApplyAsync(float time, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return;

            var characterInfo = _updateField.textInfo.characterInfo[_current];
            var meshIndex = characterInfo.materialReferenceIndex;
            var vertices = _updateField.textInfo.meshInfo[meshIndex].vertices;

            for (var vertex = 0; vertex != 4; ++vertex)
            {
                var current = characterInfo.vertexIndex + vertex;
                var origin = vertices[current];
                vertices[current] += Animating.Wobble.WobbleEffect.GenericTween(origin, time, amplitude: 0.1f);
            }
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct ConsoleTriggerEffect : IIdleAsyncEffect
    {
        void IIdleAsyncEffect.ApplyAsync(float _, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return;

            Debug.Log("Triggered");
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct FadeOutEffect : IDissolveAsyncEffect
    {
        private readonly TMP_Text _applicationField;
        private readonly int _current;

        void IDissolveAsyncEffect.ApplyAsync(float normalizedTime, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return;

            var characterInfo = _applicationField.textInfo.characterInfo[_current];
            var meshIndex = characterInfo.materialReferenceIndex;
            var meshInfo = _applicationField.textInfo.meshInfo[meshIndex];
            var newVertexColors = meshInfo.colors32;
            var modificationIndex = characterInfo.vertexIndex;

            const byte alpha = 0;
            const byte visible = 255;

            newVertexColors[modificationIndex + 0].a = Lerp(from: visible, to: alpha, normalizedTime);
            newVertexColors[modificationIndex + 1].a = Lerp(from: visible, to: alpha, normalizedTime);
            newVertexColors[modificationIndex + 2].a = Lerp(from: visible, to: alpha, normalizedTime);
            newVertexColors[modificationIndex + 3].a = Lerp(from: visible, to: alpha, normalizedTime);

            return;

            static byte Lerp(byte from, byte to, float t) => (byte) (from + t * (to - from));
        }
    }

    [InjectIgnore]
    [PolymorphicStruct]
    public readonly partial struct ApplyStrategyAlongside : IIdleAsyncEffect
    {
        private readonly IIdleAsyncEffect _first;
        private readonly IIdleAsyncEffect _second;

        void IIdleAsyncEffect.ApplyAsync(float time, CancellationToken cancellation)
        {

        }
    }

    [InjectIgnore]
    internal readonly struct FieldUpdateHandler
    {
        private readonly CancellationTokenSource _keepProcessing;

        private FieldUpdateHandler(CancellationTokenSource keepProcessing) => _keepProcessing = keepProcessing;

        internal static FieldUpdateHandler BuildAsync
        (
            TMP_Text field,
            PlayerLoopTiming yieldPoint,
            CancellationToken cancellation = default
        ) {
            var processing = CreateLinkedTokenSource(cancellation);

            TextFieldUpdateLoopAsync(field, yieldPoint, processing.Token)
                .Forget();

            return new FieldUpdateHandler(processing);
        }

        private static async UniTaskVoid TextFieldUpdateLoopAsync
        (
            TMP_Text field,
            PlayerLoopTiming yieldPoint,
            CancellationToken token = default
        ) {
            while (token.IsCancellationRequested is not true)
            {
                await UniTask.Yield(yieldPoint, token, cancelImmediately: true)
                    .SuppressCancellationThrow();

                var textInfo = field.textInfo;

                for (var i = 0; i != textInfo.meshInfo.Length; ++i)
                {
                    var meshInfo = textInfo.meshInfo[i];
                    meshInfo.mesh.colors32 = meshInfo.colors32;
                    meshInfo.mesh.vertices = meshInfo.vertices;

                    field.UpdateGeometry(meshInfo.mesh, i);
                }
            }
        }

        public UniTask DisposeAsync()
        {
            _keepProcessing.Cancel();
            _keepProcessing.Dispose();

            return UniTask.CompletedTask;
        }
    }

    [InjectIgnore]
    internal sealed class CancellationTokenDisposable : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private bool _disposed;

        public CancellationTokenDisposable(CancellationToken another)
        {
            _cancellationTokenSource = CreateLinkedTokenSource(another);
        }

        public CancellationTokenDisposable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public CancellationToken Token => _cancellationTokenSource.Token;

        public void Dispose()
        {
            if (_disposed) return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            _disposed = true;
        }
    }
}
