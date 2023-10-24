using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct Result<TValue, TExpectedError> where TExpectedError : unmanaged, IExpected
    {
        private readonly (TValue Value, bool Provided) _result;
        private readonly (Exception Error, bool Provided) _unexpected;
        private readonly (TExpectedError Error, bool Provided) _expected;

        private Result(TValue value)
        {
            _result = (value, Provided: true);
            _expected = default;
            _unexpected = default;
        }

        private Result(TExpectedError expected)
        {
            _result = default;
            _expected = (expected, Provided: true);
            _unexpected = default;
        }

        private Result(Exception unexpected)
        {
            _result = default;
            _expected = default;
            _unexpected = (unexpected, Provided: true);
        }

        public static Result<TValue, TExpectedError> Failure { get; } = new (expected: default);
        public static Result<TValue, TExpectedError> Error { get; } = new (Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (TExpectedError expected) => new (expected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (Exception unexpected) => new (unexpected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue, TExpectedError> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue, TExpectedError> FromFailure(TExpectedError failure) => failure;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TValue, TExpectedError> FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TValue, TMatch> success,
            Func<TExpectedError, TMatch> failure,
            Func<Exception, TMatch> error
        ) {
            return _result.Provided
                ? success.Invoke(_result.Value)
                : _expected.Provided
                    ? failure.Invoke(_expected.Error)
                    : error.Invoke(_unexpected.Error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match
        (
            Func<TValue, TValue> success,
            Func<TExpectedError, TValue> failure,
            Func<Exception, TValue> error
        ) {
            return _result.Provided
                ? success.Invoke(_result.Value)
                :_expected.Provided
                    ? failure.Invoke(_expected.Error)
                    : error.Invoke(_unexpected.Error);
        }
    }
}
