using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using static Outcome.Expected;

    [BurstCompile]
    public readonly struct AsyncResult<TValue>
    {
        internal readonly (Exception Value, bool Provided) Error;
        internal readonly (Cancellation Value, bool Provided) Cancellation;
        internal readonly (TValue Value, bool Provided) Income;

        public AsyncResult(TValue value)
        {
            Income = (value, true);
            Error = default;
            Cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            Income = default;
            Error = (error, Provided: true);
            Cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            Income = default;
            Error = default;
            Cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult<TValue> FromResult(TValue value) => new(value);
        public static AsyncResult<TValue> Failure { get; } = new (Outcome.Unexpected.Error);
        public static AsyncResult<TValue> Cancel { get; } = new (Canceled);

        public bool IsSuccessful => Income is { Provided: true };
        public bool IsFailure => Error is { Provided: true };
        public bool IsCancellation => Cancellation is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (TValue value) => new (value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (Exception error) => new (error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (CancellationToken _) => Cancel;

        public static AsyncResult<TValue> Produce(Func<TValue> from, CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return Cancel;

            try
            {
                return from.Invoke();
            }
            catch (Exception exception)
            {
                return new AsyncResult<TValue>(exception);
            }
        }

        public static UniTask<AsyncResult<TValue>> ProduceAsync
        (
            Func<CancellationToken, UniTask<AsyncResult<TValue>>> from,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(Cancel);

            return from.Invoke(cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.Value, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMap> MapAsync<TMap>
        (
            Func<TValue, CancellationToken, UniTask<TMap>> success,
            Func<CancellationToken, UniTask<TMap>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMap>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.Value, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult> RunAsync<TAnother>
        (
            Func<TValue, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.Value, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : new AsyncResult(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TValue, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.Value, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : new AsyncResult<TAnother>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TValue, TAnother> Attach<TAnother>(TAnother scope)
        {
            if (IsSuccessful) return new AsyncResult<TValue, TAnother>(Income.Value, scope);

            return IsCancellation
                ? AsyncResult<TValue, TAnother>.Cancel
                : new AsyncResult<TValue, TAnother>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TValue, TFirst, TSecond> Attach<TFirst, TSecond>(TFirst first, TSecond second)
        {
            if (IsSuccessful) return new AsyncResult<TValue, TFirst, TSecond>(Income.Value, first, second);

            return IsCancellation
                ? AsyncResult<TValue, TFirst, TSecond>.Cancel
                : new AsyncResult<TValue, TFirst, TSecond>(Error.Value);
        }
    }

    [BurstCompile]
    public readonly struct AsyncResult<TFirst, TSecond>
    {
        internal readonly (Exception Value, bool Provided) Error;
        internal readonly (Cancellation Value, bool Provided) Cancellation;
        internal readonly (TFirst First, TSecond Second, bool Provided) Income;

        internal AsyncResult(TFirst first, TSecond second)
        {
            Income = (first, second, true);
            Error = default;
            Cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            Income = default;
            Error = (error, Provided: true);
            Cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            Income = default;
            Error = default;
            Cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult<TFirst, TSecond>  FromResult(TFirst first, TSecond second) => new (first, second);
        public static AsyncResult<TFirst, TSecond> Failure { get; } = new (Outcome.Unexpected.Error);
        public static AsyncResult<TFirst, TSecond> Cancel { get; } = new (Canceled);

        public bool IsSuccessful => Income is { Provided: true };
        public bool IsFailure => Error is { Provided: true };
        public bool IsCancellation => Cancellation is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (ValueTuple<TFirst, TSecond> income) => new (income.Item1, income.Item2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (Exception error) => new (error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (CancellationToken _) => Cancel;

        public static AsyncResult<TFirst, TSecond> Produce(Func<(TFirst, TSecond)> from, CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return Cancel;

            try
            {
                return from.Invoke();
            }
            catch (Exception exception)
            {
                return new AsyncResult<TFirst, TSecond>(exception);
            }
        }

        public static UniTask<AsyncResult<TFirst, TSecond>> ProduceAsync
        (
            Func<CancellationToken, UniTask<AsyncResult<TFirst, TSecond>>> from,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(Cancel);

            return from.Invoke(cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.First, Income.Second, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMap> MapAsync<TMap>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<TMap>> success,
            Func<CancellationToken, UniTask<TMap>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMap>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.First, Income.Second, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TAnother> Run<TAnother>(Func<TFirst, TSecond, AsyncResult<TAnother>> run)
        {
            if (IsSuccessful) return run.Invoke(Income.First, Income.Second);
            if (IsCancellation) return AsyncResult<TAnother>.Cancel;

            return new AsyncResult<TAnother>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : new AsyncResult(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : new AsyncResult<TAnother>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TFirst, TSecond, TThird> Attach<TThird>(TThird additional)
        {
            if (IsSuccessful) return new AsyncResult<TFirst, TSecond, TThird>(Income.First, Income.Second, additional);

            return IsCancellation
                ? AsyncResult<TFirst, TSecond, TThird>.Cancel
                : new AsyncResult<TFirst, TSecond, TThird>(Error.Value);
        }
    }

    [BurstCompile]
    public readonly struct AsyncResult<TFirst, TSecond, TThird>
    {
        internal readonly (Exception Value, bool Provided) Error;
        internal readonly (Cancellation Value, bool Provided) Cancellation;
        internal readonly (TFirst First, TSecond Second, TThird Third, bool Provided) Income;

        internal AsyncResult(TFirst first, TSecond second, TThird third)
        {
            Income = (first, second, third, true);
            Error = default;
            Cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            Income = default;
            Error = (error, Provided: true);
            Cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            Income = default;
            Error = default;
            Cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult<TFirst, TSecond, TThird> Failure { get; } = new (Outcome.Unexpected.Error);
        public static AsyncResult<TFirst, TSecond, TThird> Cancel { get; } = new (Canceled);

        public bool IsSuccessful => Income is { Provided: true };
        public bool IsFailure => Error is { Provided: true };
        public bool IsCancellation => Cancellation is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (ValueTuple<TFirst, TSecond, TThird> income) => new (income.Item1, income.Item2, income.Item3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (Exception error) => new (error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (CancellationToken _) => Cancel;

        public static AsyncResult<TFirst, TSecond, TThird> Produce
        (
            Func<(TFirst, TSecond, TThird)> from,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Cancel;

            try
            {
                return from.Invoke();
            }
            catch (Exception exception)
            {
                return new AsyncResult<TFirst, TSecond, TThird>(exception);
            }
        }

        public static UniTask<AsyncResult<TFirst, TSecond, TThird>> ProduceAsync
        (
            Func<CancellationToken, UniTask<AsyncResult<TFirst, TSecond, TThird>>> from,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(Cancel);

            return from.Invoke(cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.First, Income.Second, Income.Third, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMap> MapAsync<TMap>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<TMap>> success,
            Func<CancellationToken, UniTask<TMap>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMap>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(Income.First, Income.Second, Income.Third, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, Income.Third, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : new AsyncResult(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, Income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : new AsyncResult<TAnother>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother, TAnotherOne>> RunAsync<TAnother, TAnotherOne>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother, TAnotherOne>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, Income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother, TAnotherOne>.Cancel
                : new AsyncResult<TAnother, TAnotherOne>(Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother, TAnotherOne, TYetAnother>> RunAsync<TAnother, TAnotherOne, TYetAnother>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother, TAnotherOne, TYetAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(Income.First, Income.Second, Income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother, TAnotherOne, TYetAnother>.Cancel
                : new AsyncResult<TAnother, TAnotherOne, TYetAnother>(Error.Value);
        }
    }

    public static class AsyncResultFromUniTaskExecution
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<TMap> MapAsync<TValue, TMap>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            Func<TValue, CancellationToken, UniTask<TMap>> success,
            Func<CancellationToken, UniTask<TMap>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMap>> failure,
            CancellationToken token = default
        ) {
            var result = await candidate;

            if (result.IsSuccessful)
            {
                return await success.Invoke(result.Income.Value, token);
            }
            else if (result.IsCancellation)
            {
                return await cancellation.Invoke(token);
            }
            else
            {
                return await failure.Invoke(result.Error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TValue, TResulted>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            Func<TValue, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = await candidate;

            if (result.IsSuccessful) return await run.Invoke(result.Income.Value, cancellation);

            return result.IsCancellation
                ? AsyncResult<TResulted>.Cancel
                : new AsyncResult<TResulted>(result.Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TFirst, TSecond, TResulted>
        (
            this UniTask<AsyncResult<TFirst, TSecond>> candidate,
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = await candidate;

            if (result.IsSuccessful) return await run.Invoke(result.Income.First, result.Income.Second, cancellation);

            return result.IsCancellation
                ? AsyncResult<TResulted>.Cancel
                : new AsyncResult<TResulted>(result.Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TFirst, TSecond, TThird, TResulted>
        (
            this UniTask<AsyncResult<TFirst, TSecond, TThird>> candidate,
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = await candidate;

            if (result.IsSuccessful) return await run.Invoke(result.Income.First, result.Income.Second, result.Income.Third, cancellation);

            return result.IsCancellation
                ? AsyncResult<TResulted>.Cancel
                : new AsyncResult<TResulted>(result.Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TValue, TFirst>> AttachAsync<TValue, TFirst>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            TFirst first,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested)
                return AsyncResult<TValue, TFirst>.Cancel;

            var result = await candidate;

            if (result.IsSuccessful) return new AsyncResult<TValue, TFirst>
            (
                result.Income.Value, first
            );

            return result.IsCancellation
                ? AsyncResult<TValue, TFirst>.Cancel
                : new AsyncResult<TValue, TFirst>(result.Error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TValue, TFirst, TSecond>> AttachAsync<TValue, TFirst, TSecond>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            TFirst first,
            TSecond second,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested)
                return AsyncResult<TValue, TFirst, TSecond>.Cancel;

            var result = await candidate;

            if (result.IsSuccessful) return new AsyncResult<TValue, TFirst, TSecond>
            (
                result.Income.Value, first, second
            );

            return result.IsCancellation
                ? AsyncResult<TValue, TFirst, TSecond>.Cancel
                : new AsyncResult<TValue, TFirst, TSecond>(result.Error.Value);
        }
    }
}
