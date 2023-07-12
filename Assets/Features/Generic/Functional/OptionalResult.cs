using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct OptionalResult<TValue>
    {
        private readonly bool _successful;
        private readonly bool _hasSome;

        private readonly TValue _value;
        private readonly Exception _error;

        public OptionalResult(TValue value)
        {
            _successful = true;
            _hasSome = true;

            _value = value;
            _error = default;
        }

        public OptionalResult(Exception error)
        {
            _successful = false;
            _hasSome = false;

            _value = default;
            _error = error;
        }

        private OptionalResult(NoneResult none = default)
        {
            _successful = true;
            _hasSome = false;

            _value = default;
            _error = default;
        }

        public static OptionalResult<TValue> None { get; } = new (none: default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalResult<TValue> (Exception error) => new (error);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TValue, TMatch> something, Func<TMatch> nothing, Func<Exception, TMatch> failure)
        {
            return _successful
                ? _hasSome
                    ? something.Invoke(_value)
                    : nothing.Invoke()
                : failure.Invoke(_error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match(Func<TValue, TValue> something, Func<TValue> nothing, Func<Exception, TValue> failure)
        {
            return _successful
                ? _hasSome
                    ? something.Invoke(_value)
                    : nothing.Invoke()
                : failure.Invoke(_error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match<TMatch>(Action<TValue> something, Action nothing, Action<Exception> failure)
        {
            if (_successful)
            {
                if (_hasSome)
                {
                    something.Invoke(_value);
                }
                else
                {
                    nothing.Invoke();
                }
            }
            else
            {
                failure.Invoke(_error);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> something,
            Func<CancellationToken, UniTask<TMatch>> nothing,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _hasSome
                    ? something.Invoke(_value, cancellation)
                    : nothing.Invoke(cancellation)
                : failure.Invoke(_error, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TValue> MatchAsync
        (
            Func<TValue, CancellationToken, UniTask<TValue>> something,
            Func<CancellationToken, UniTask<TValue>> nothing,
            Func<Exception, CancellationToken, UniTask<TValue>> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _hasSome
                    ? something.Invoke(_value, cancellation)
                    : nothing.Invoke(cancellation)
                : failure.Invoke(_error, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TValue, CancellationToken, UniTask> something,
            Func<CancellationToken, UniTask> nothing,
            Func<Exception, CancellationToken, UniTask> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? _hasSome
                    ? something.Invoke(_value, cancellation)
                    : nothing.Invoke(cancellation)
                : failure.Invoke(_error, cancellation);
        }

        [BurstCompile]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneResult { }
    }
}
