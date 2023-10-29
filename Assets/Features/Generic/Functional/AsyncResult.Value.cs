using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct AsyncResult<TValue>
    {
        private readonly (TValue Value, bool Provided) _income;
        private readonly (CancellationToken Value, bool Provided) _cancellation;
        private readonly (Exception Value, bool Provided) _exception;

        private AsyncResult(TValue value)
        {
            _income = (value, Provided: true);
            _cancellation = default;
            _exception = default;
        }

        private AsyncResult(CancellationToken cancellation)
        {
            _income = default;
            _cancellation = (cancellation, Provided: true);
            _exception = default;
        }

        private AsyncResult(Exception error)
        {
            _income = default;
            _cancellation = default;
            _exception = (error, Provided: true);
        }

        public static AsyncResult<TValue> Cancel { get; } = new (CancellationToken.None);
        public static AsyncResult<TValue> Error { get; } = new (Unexpected.Error);
        public static AsyncResult<TValue> Impossible { get; } = new (Unexpected.Impossible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (TValue value) => new (value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (Exception exception) => new (exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TValue> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TValue> FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TValue> FromException(Exception error) => error;

        private bool IsSuccessful => _income.Provided;
        private bool IsCancellation => _cancellation.Provided;
        private bool IsError => _exception.Provided;

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
        public AsyncResult<TValue, TAnother> Attach<TAnother>(TAnother scope)
        {
            if (IsSuccessful) return AsyncResult<TValue, TAnother>.FromResult(_income.Value, scope);

            return IsCancellation
                ? AsyncResult<TValue, TAnother>.Cancel
                : AsyncResult<TValue, TAnother>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TValue, TFirst, TSecond> Attach<TFirst, TSecond>(TFirst first, TSecond second)
        {
            if (IsSuccessful) return AsyncResult<TValue, TFirst, TSecond>.FromResult(_income.Value, first, second);

            return IsCancellation
                ? AsyncResult<TValue, TFirst, TSecond>.Cancel
                : AsyncResult<TValue, TFirst, TSecond>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TValue, CancellationToken, UniTask> success,
            Func<CancellationToken, UniTask> cancellation,
            Func<Exception, CancellationToken, UniTask> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(_income.Value, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return error.Invoke(_exception.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(_income.Value, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return error.Invoke(_exception.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TValue, CancellationToken, TMatch> success,
            Func<CancellationToken, TMatch> cancellation,
            Func<Exception, CancellationToken, TMatch> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(_income.Value, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return error.Invoke(_exception.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult Run
        (
            Func<TValue, CancellationToken, AsyncResult> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return run.Invoke(_income.Value, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : AsyncResult.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TAnother> Run<TAnother>
        (
            Func<TValue, CancellationToken, AsyncResult<TAnother>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return run.Invoke(_income.Value, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : AsyncResult<TAnother>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult> RunAsync
        (
            Func<TValue, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.Value, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : AsyncResult.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TValue, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.Value, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : AsyncResult<TAnother>.FromException(_exception.Value);
        }
    }

    [BurstCompile]
    public readonly struct AsyncResult<TFirst, TSecond>
    {
        private readonly (TFirst First, TSecond Second, bool Provided) _income;
        private readonly (CancellationToken Value, bool Provided) _cancellation;
        private readonly (Exception Value, bool Provided) _exception;

        private AsyncResult(TFirst first, TSecond second)
        {
            _income = (first, second, Provided: true);
            _cancellation = default;
            _exception = default;
        }

        private AsyncResult(CancellationToken cancellation)
        {
            _income = default;
            _cancellation = (cancellation, Provided: true);
            _exception = default;
        }

        private AsyncResult(Exception exception)
        {
            _income = default;
            _cancellation = default;
            _exception = (exception, Provided: true);
        }

        public static AsyncResult<TFirst, TSecond> Error { get; } = new (Unexpected.Error);
        public static AsyncResult<TFirst, TSecond> Cancel { get; } = new (CancellationToken.None);
        public static AsyncResult<TFirst, TSecond> Impossible { get; } = new (Unexpected.Impossible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (ValueTuple<TFirst, TSecond> income) => new (income.Item1, income.Item2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond> (Exception exception) => new (exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond>  FromResult(TFirst first, TSecond second) => new (first, second);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond>  FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond>  FromException(Exception error) => error;

        private bool IsSuccessful => _income.Provided;
        private bool IsCancellation => _cancellation.Provided;
        private bool IsError => _exception.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond> Produce
        (
            Func<(TFirst, TSecond)> from,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return Cancel;

            try
            {
                return from.Invoke();
            }
            catch (Exception exception)
            {
                return FromException(exception);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public AsyncResult<TFirst, TSecond, TThird> Attach<TThird>(TThird additional)
        {
            if (IsSuccessful) return AsyncResult<TFirst, TSecond, TThird>.FromResult(_income.First, _income.Second, additional);

            return IsCancellation
                ? AsyncResult<TFirst, TSecond, TThird>.Cancel
                : AsyncResult<TFirst, TSecond, TThird>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult Run
        (
            Func<TFirst, TSecond, CancellationToken, AsyncResult> run,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return AsyncResult.Cancel;
            if (IsSuccessful) return run.Invoke(_income.First, _income.Second, cancellation);

            return AsyncResult.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TAnother> Run<TAnother>
        (
            Func<TFirst, TSecond, CancellationToken, AsyncResult<TAnother>> run,
            CancellationToken cancellation = default
        ) {
            if (cancellation.IsCancellationRequested) return AsyncResult<TAnother>.Cancel;
            if (IsSuccessful) return run.Invoke(_income.First, _income.Second, cancellation);

            return AsyncResult<TAnother>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult> RunAsync
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : AsyncResult.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : AsyncResult<TAnother>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother1, TAnother2>> RunAsync<TAnother1, TAnother2>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TAnother1, TAnother2>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother1, TAnother2>.Cancel
                : AsyncResult<TAnother1, TAnother2>.FromException(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, TSecond, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(_income.First, _income.Second, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return error.Invoke(_exception.Value, token);
            }
        }
    }

    [BurstCompile]
    public readonly struct AsyncResult<TFirst, TSecond, TThird>
    {
        private readonly (TFirst First, TSecond Second, TThird Third, bool Provided) _income;
        private readonly (CancellationToken Value, bool Provided) _cancellation;
        private readonly (Exception Value, bool Provided) _error;

        private AsyncResult(TFirst first, TSecond second, TThird third)
        {
            _income = (first, second, third, true);
            _cancellation = default;
            _error = default;
        }

        private AsyncResult(CancellationToken cancellation)
        {
            _income = default;
            _cancellation = (cancellation, Provided: true);
            _error = default;
        }

        private AsyncResult(Exception exception)
        {
            _income = default;
            _cancellation = default;
            _error = (exception, Provided: true);
        }

        public static AsyncResult<TFirst, TSecond, TThird> Cancel { get; } = new (CancellationToken.None);
        public static AsyncResult<TFirst, TSecond, TThird> Error { get; } = new (Unexpected.Error);
        public static AsyncResult<TFirst, TSecond, TThird> Impossible { get; } = new (Unexpected.Impossible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (ValueTuple<TFirst, TSecond, TThird> income) => new (income.Item1, income.Item2, income.Item3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TFirst, TSecond, TThird> (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond, TThird>  FromResult(TFirst first, TSecond second, TThird third) => new (first, second, third);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond, TThird>  FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<TFirst, TSecond, TThird>  FromException(Exception error) => error;

        private bool IsSuccessful => _income.Provided;
        private bool IsFailure => _error.Provided;
        private bool IsCancellation => _cancellation.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return FromException(exception);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public async UniTask<AsyncResult> RunAsync
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, _income.Third, cancellation);

            return IsCancellation
                ? AsyncResult.Cancel
                : AsyncResult.FromException(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother>> RunAsync<TAnother>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, _income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother>.Cancel
                : AsyncResult<TAnother>.FromException(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother, TAnotherOne>> RunAsync<TAnother, TAnotherOne>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother, TAnotherOne>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, _income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother, TAnotherOne>.Cancel
                : AsyncResult<TAnother, TAnotherOne>.FromException(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<AsyncResult<TAnother, TAnotherOne, TYetAnother>> RunAsync<TAnother, TAnotherOne, TYetAnother>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TAnother, TAnotherOne, TYetAnother>>> run,
            CancellationToken cancellation = default
        ) {
            if (IsSuccessful) return await run.Invoke(_income.First, _income.Second, _income.Third, cancellation);

            return IsCancellation
                ? AsyncResult<TAnother, TAnotherOne, TYetAnother>.Cancel
                : AsyncResult<TAnother, TAnotherOne, TYetAnother>.FromException(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(_income.First, _income.Second, _income.Third, token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return error.Invoke(_error.Value, token);
            }
        }
    }

    public static class AsyncResultFromUniTaskExecution
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TValue, TAnother>> AttachAsync<TValue, TAnother>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            TAnother another,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TValue>.Cancel;

            return result.Attach(another);
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
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TValue>.Cancel;

            return result.Attach(first, second);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TFirst, TSecond, TAnother>> AttachAsync<TFirst, TSecond, TAnother>
        (
            this UniTask<AsyncResult<TFirst, TSecond>> candidate,
            TAnother another,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TFirst, TSecond>.Cancel;

            return result.Attach(another);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TValue, TResulted>
        (
            this UniTask<AsyncResult<TValue>> candidate,
            Func<TValue, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TValue>.Cancel;

            return await result.RunAsync(run, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TFirst, TSecond, TResulted>
        (
            this UniTask<AsyncResult<TFirst, TSecond>> candidate,
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TFirst, TSecond>.Cancel;

            return await result.RunAsync(run, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted>> RunAsync<TFirst, TSecond, TThird, TResulted>
        (
            this UniTask<AsyncResult<TFirst, TSecond, TThird>> candidate,
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TResulted>>> run,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TFirst, TSecond, TThird>.Cancel;

            return await result.RunAsync(run, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted1, TResulted2>> RunAsync<TFirst, TSecond, TThird, TResulted1, TResulted2>
        (
            this UniTask<AsyncResult<TFirst, TSecond, TThird>> candidate,
            Func<TFirst, TSecond, TThird, CancellationToken, UniTask<AsyncResult<TResulted1, TResulted2>>> run,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TFirst, TSecond, TThird>.Cancel;

            return await result.RunAsync(run, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<AsyncResult<TResulted1, TResulted2>> RunAsync<TFirst, TSecond, TResulted1, TResulted2>
        (
            this UniTask<AsyncResult<TFirst, TSecond>> candidate,
            Func<TFirst, TSecond, CancellationToken, UniTask<AsyncResult<TResulted1, TResulted2>>> run,
            CancellationToken cancellation = default
        ) {
            var result = cancellation.IsCancellationRequested is false
                ? await candidate
                : AsyncResult<TFirst, TSecond>.Cancel;

            return await result.RunAsync(run, cancellation);
        }
    }
}
