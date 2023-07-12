using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    internal readonly struct Result<TValue, TExpectedError> where TExpectedError : IExpected
    {
        private readonly (TValue Value, bool Provided) _result;
        private readonly (Exception Error, bool Provided) _unexpected;
        private readonly (TExpectedError Error, bool Provided) _expected;

        public Result(TValue value)
        {
            _result = (value, Provided: true);
            _expected = default;
            _unexpected = default;
        }

        public Result(TExpectedError expected)
        {
            _result = default;
            _expected = (expected, Provided: true);
            _unexpected = default;
        }

        public Result(Exception unexpected)
        {
            _result = default;
            _expected = default;
            _unexpected = (unexpected, Provided: true);
        }

        public static Result<TValue, TExpectedError> Unexpected { get; } = new (Outcome.Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (TExpectedError expected) => new (expected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue, TExpectedError> (Exception unexpected) => new (unexpected);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TValue, TMatch> success,
            Func<TExpectedError, TMatch> expected,
            Func<Exception, TMatch> unexpected
        ) {
            return _result is { Provided: true }
                ? success.Invoke(_result.Value)
                : _expected is { Provided: true }
                    ? expected.Invoke(_expected.Error)
                    : unexpected.Invoke(_unexpected.Error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match
        (
            Func<TValue, TValue> success,
            Func<TExpectedError, TValue> expected,
            Func<Exception, TValue> unexpected
        ) {
            return _result is { Provided: true }
                ? success.Invoke(_result.Value)
                : _expected is { Provided: true }
                    ? expected.Invoke(_expected.Error)
                    : unexpected.Invoke(_unexpected.Error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<TExpectedError, CancellationToken, UniTask<TMatch>> expected,
            Func<Exception, CancellationToken, UniTask<TMatch>> unexpected,
            CancellationToken cancellation = default
        ) {
            return _result is { Provided: true }
                ? success.Invoke(_result.Value, cancellation)
                : _expected is { Provided: true }
                    ? expected.Invoke(_expected.Error, cancellation)
                    : unexpected.Invoke(_unexpected.Error, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TValue> MatchAsync
        (
            Func<TValue, CancellationToken, UniTask<TValue>> success,
            Func<TExpectedError, CancellationToken, UniTask<TValue>> expected,
            Func<Exception, CancellationToken, UniTask<TValue>> unexpected,
            CancellationToken cancellation = default
        ) {
            return _result is { Provided: true }
                ? success.Invoke(_result.Value, cancellation)
                : _expected is { Provided: true }
                    ? expected.Invoke(_expected.Error, cancellation)
                    : unexpected.Invoke(_unexpected.Error, cancellation);
        }
    }
}
