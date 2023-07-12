using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    internal readonly partial struct Result
    {
        private readonly (Exception Value, bool Provided) _error;

        private Result(NoneResult value = default) => _error = default;
        public Result(Exception error) => _error = (error, Provided: true);

        public static Result Success { get; } = new ();
        public static Result Failure { get; } = new (Outcome.Unexpected.Error);

        public bool IsSuccessful => _error is { Provided: false };
        public bool IsFailure => _error is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result (Exception error) => new (error);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TMatch> success, Func<Exception, TMatch> failure)
        {
            return IsSuccessful
                ? success.Invoke()
                : failure.Invoke(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action success, Action<Exception> failure)
        {
            if (IsSuccessful)
            {
                success.Invoke();
            }
            else
            {
                failure.Invoke(_error.Value);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<CancellationToken, UniTask<TMatch>> success,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return IsSuccessful
                ? success.Invoke(cancellation)
                : failure.Invoke(_error.Value, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<CancellationToken, UniTask> success,
            Func<Exception, CancellationToken, UniTask> failure,
            CancellationToken cancellation = default
        ) {
            return IsSuccessful
                ? success.Invoke(cancellation)
                : failure.Invoke(_error.Value, cancellation);
        }

        [BurstCompile]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneResult { }
    }

    internal static partial class ResultAsyncExtensions
    {
        public static async UniTask<TMatch> MatchAsync<TMatch>
        (
            this UniTask<Result> asyncResult,
            Func<CancellationToken, UniTask<TMatch>> success,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken cancellation = default
        ) {
            return await (await asyncResult).MatchAsync(success, failure, cancellation);
        }

        public static async UniTask MatchAsync
        (
            this UniTask<Result> asyncResult,
            Func<CancellationToken, UniTask> success,
            Func<Exception, CancellationToken, UniTask> failure,
            CancellationToken cancellation = default
        ) {
            await (await asyncResult).MatchAsync(success, failure, cancellation);
        }
    }
}
