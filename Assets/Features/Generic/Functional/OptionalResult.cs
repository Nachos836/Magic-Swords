using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct OptionalResult<TValue>
    {
        private readonly bool _successful;

        private readonly (bool Provided, TValue Value) _income;
        private readonly (bool Provided, Exception Exception) _error;

        private OptionalResult(NoneResult none = default)
        {
            _successful = true;

            _income = default;
            _error = default;
        }

        private OptionalResult(TValue value)
        {
            _successful = true;

            _income = (Provided: true, value);
            _error = default;
        }

        private OptionalResult(Exception error)
        {
            _successful = false;

            _income = default;
            _error = (Provided: true, error);
        }

        public static OptionalResult<TValue> None { get; } = new (none: default);
        public static OptionalResult<TValue> Error { get; } = new (Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TValue> (Exception exception) => new (exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalResult<TValue> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalResult<TValue> FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OptionalResult<TValue, TAnotherValue> Attach<TAnotherValue>(TAnotherValue another)
        {
            return _successful
                ? _income.Provided
                    ? OptionalResult<TValue, TAnotherValue>.FromResult(_income.Value, another)
                    : OptionalResult<TValue, TAnotherValue>.None
                : OptionalResult<TValue, TAnotherValue>.FromException(_error.Exception);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TValue, TMatch> something,
            Func<TMatch> nothing,
            Func<Exception, TMatch> error
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.Value)
                    : nothing.Invoke()
                : error.Invoke(_error.Exception);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action<TValue> something, Action nothing, Action<Exception> error)
        {
            if (_successful)
            {
                if (_income.Provided)
                {
                    something.Invoke(_income.Value);
                }
                else
                {
                    nothing.Invoke();
                }
            }
            else
            {
                error.Invoke(_error.Exception);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> something,
            Func<CancellationToken, UniTask<TMatch>> nothing,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.Value, cancellation)
                    : nothing.Invoke(cancellation)
                : error.Invoke(_error.Exception, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TValue, CancellationToken, UniTask> something,
            Func<CancellationToken, UniTask> nothing,
            Func<Exception, CancellationToken, UniTask> error,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.Value, cancellation)
                    : nothing.Invoke(cancellation)
                : error.Invoke(_error.Exception, cancellation);
        }

        [BurstCompile]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneResult { }
    }

    [BurstCompile]
    public readonly struct OptionalResult<TFirstValue, TSecondValue>
    {
        private readonly bool _successful;

        private readonly (bool Provided, TFirstValue FirstValue, TSecondValue SecondValueValue) _income;
        private readonly (bool Provided, Exception Exception) _error;

        private OptionalResult(TFirstValue firstValue, TSecondValue secondValueValue)
        {
            _successful = true;

            _income = (Provided: true, firstValue, secondValueValue);
            _error = default;
        }

        private OptionalResult(Exception error)
        {
            _successful = false;

            _income = default;
            _error = (Provided: true, error);
        }

        private OptionalResult(NoneResult none = default)
        {
            _successful = true;

            _income = default;
            _error = default;
        }

        public static OptionalResult<TFirstValue, TSecondValue> None { get; } = new (none: default);
        public static OptionalResult<TFirstValue, TSecondValue> Error { get; } = new (Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TFirstValue, TSecondValue> ((TFirstValue FirstValue, TSecondValue SecondValue) income) => FromResult(income.FirstValue, income.SecondValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TFirstValue, TSecondValue> (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalResult<TFirstValue, TSecondValue> FromResult(TFirstValue firstValue, TSecondValue secondValue) => new (firstValue, secondValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalResult<TFirstValue, TSecondValue> FromException(Exception exception) => exception;

        public AsyncResult<TResult> Run<TResult>
        (
            Func<TFirstValue, TSecondValue, CancellationToken, AsyncResult<TResult>> whenSome,
            Func<CancellationToken, AsyncResult<TResult>> whenNone,
            CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested) return AsyncResult<TResult>.Cancel;

            return _successful
                ? _income.Provided
                    ? whenSome.Invoke(_income.FirstValue, _income.SecondValueValue, cancellation)
                    : whenNone.Invoke(cancellation)
                : AsyncResult<TResult>.FromException(_error.Exception);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TFirstValue, TSecondValue, TMatch> something,
            Func<TMatch> nothing,
            Func<Exception, TMatch> failure
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.FirstValue, _income.SecondValueValue)
                    : nothing.Invoke()
                : failure.Invoke(_error.Exception);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<TFirstValue, TSecondValue> something,
            Action nothing, Action<Exception> failure
        ) {
            if (_successful)
            {
                if (_income.Provided)
                {
                    something.Invoke(_income.FirstValue, _income.SecondValueValue);
                }
                else
                {
                    nothing.Invoke();
                }
            }
            else
            {
                failure.Invoke(_error.Exception);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirstValue, TSecondValue, CancellationToken, UniTask<TMatch>> something,
            Func<CancellationToken, UniTask<TMatch>> nothing,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.FirstValue, _income.SecondValueValue, cancellation)
                    : nothing.Invoke(cancellation)
                : failure.Invoke(_error.Exception, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TFirstValue, TSecondValue, CancellationToken, UniTask> something,
            Func<CancellationToken, UniTask> nothing,
            Func<Exception, CancellationToken, UniTask> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _income.Provided
                    ? something.Invoke(_income.FirstValue, _income.SecondValueValue, cancellation)
                    : nothing.Invoke(cancellation)
                : failure.Invoke(_error.Exception, cancellation);
        }

        [BurstCompile]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneResult { }
    }
}
