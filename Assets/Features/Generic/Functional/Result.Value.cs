using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    internal readonly struct Result<TValue>
    {
        private readonly bool _successful;

        private readonly TValue _value;
        private readonly Exception _error;

        public Result(TValue value)
        {
            _successful = true;

            _value = value;
            _error = default;
        }

        public Result(Exception error)
        {
            _successful = false;

            _value = default;
            _error = error;
        }

        public static Result<TValue> Failure { get; } = new (Outcome.Unexpected.Error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result<TValue> (Exception error) => new (error);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TValue, TMatch> success, Func<Exception, TMatch> failure)
        {
            return _successful
                ? success.Invoke(_value)
                : failure.Invoke(_error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Match(Func<TValue, TValue> success, Func<Exception, TValue> failure)
        {
            return _successful
                ? success.Invoke(_value)
                : failure.Invoke(_error);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? success.Invoke(_value, cancellation)
                : failure.Invoke(_error, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TValue> MatchAsync
        (
            Func<TValue, CancellationToken, UniTask<TValue>> success,
            Func<Exception, CancellationToken, UniTask<TValue>> failure,
            CancellationToken cancellation = default
        ) {
            return _successful
                ? success.Invoke(_value, cancellation)
                : failure.Invoke(_error, cancellation);
        }
    }

    internal static partial class ResultAsyncExtensions
    {
        public static async UniTask<TMatch> MatchAsync<TMatch, TValue>
        (
            this UniTask<Result<TValue>> asyncResult,
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return await (await asyncResult).MatchAsync(success, failure, cancellation);
        }

        public static async UniTask<TValue> MatchAsync<TValue>
        (
            this UniTask<Result<TValue>> asyncResult,
            Func<TValue, CancellationToken, UniTask<TValue>> success,
            Func<Exception, CancellationToken, UniTask<TValue>> failure,
            CancellationToken cancellation = default
        ) {
            return await (await asyncResult).MatchAsync(success, failure, cancellation);
        }
    }
}
