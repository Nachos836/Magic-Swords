using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct RichResult<TValue>
    {
        private readonly (TValue Value, bool Provided) _result;
        private readonly (Exception Error, bool Provided) _unexpected;
        private readonly (Expected.Failure Failure, bool Provided) _expected;

        private RichResult(TValue value)
        {
            _result = (value, Provided: true);
            _expected = default;
            _unexpected = default;
        }

        private RichResult(Expected.Failure expected)
        {
            _result = default;
            _expected = (expected, Provided: true);
            _unexpected = default;
        }

        private RichResult(Exception unexpected)
        {
            _result = default;
            _expected = default;
            _unexpected = (unexpected, Provided: true);
        }

        public static RichResult<TValue> Failure { get; } = new (Expected.Failed);
        public static RichResult<TValue> Error { get; } = new (Unexpected.Error);
        public static RichResult<TValue> Impossible { get; } = new (Unexpected.Impossible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RichResult<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RichResult<TValue> (Expected.Failure expected) => new (expected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RichResult<TValue> (Exception unexpected) => new (unexpected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RichResult<TValue> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RichResult<TValue> FromFailure(Expected.Failure failure) => failure;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RichResult<TValue> FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TValue, TMatch> success,
            Func<Expected.Failure, TMatch> failure,
            Func<Exception, TMatch> error
        ) {
            return _result.Provided
                ? success.Invoke(_result.Value)
                : _expected.Provided
                    ? failure.Invoke(_expected.Failure)
                    : error.Invoke(_unexpected.Error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match
        (
            Func<TValue, TValue> success,
            Func<Expected.Failure, TValue> failure,
            Func<Exception, TValue> error
        ) {
            return _result.Provided
                ? success.Invoke(_result.Value)
                :_expected.Provided
                    ? failure.Invoke(_expected.Failure)
                    : error.Invoke(_unexpected.Error);
        }
    }
}
