using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct Result<TValue>
    {
        private readonly (bool Provided, TValue Value) _income;
        private readonly (bool Provided, Exception Value) _exception;

        private Result(TValue value)
        {
            _income = (Provided: true, value);
            _exception = default;
        }

        private Result(Exception exception)
        {
            _income = default;
            _exception = (Provided: true, exception);
        }

        public static Result<TValue> Error { get; } = new (Outcome.Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue> (Exception exception) => new (exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue> FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TValue, TMatch> success, Func<Exception, TMatch> error)
        {
            return _income.Provided
                ? success.Invoke(_income.Value)
                : error.Invoke(_exception.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match(Func<TValue, TValue> success, Func<Exception, TValue> error)
        {
            return _income.Provided
                ? success.Invoke(_income.Value)
                : error.Invoke(_exception.Value);
        }
    }
}
