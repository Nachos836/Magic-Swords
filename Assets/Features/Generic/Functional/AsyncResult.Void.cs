using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using static Outcome.Expected;
    using static Outcome.Unexpected;

    [BurstCompile]
    public readonly struct AsyncResult
    {
        private readonly (Exception Value, bool Provided) _error;
        private readonly (Cancellation Value, bool Provided) _cancellation;

        private AsyncResult(bool success = true)
        {
            IsSuccessful = success;
            _error = default;
            _cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            IsSuccessful = false;
            _error = (error, Provided: true);
            _cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            IsSuccessful = false;
            _error = default;
            _cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult Success { get; } = new (success: true);
        public static AsyncResult Failure { get; } = new (Error);
        public static AsyncResult Cancel { get; } = new (Canceled);

        public bool IsSuccessful { get; }
        public bool IsFailure => _error is { Provided: true };
        public bool IsCancellation => _cancellation is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult (Exception error) => new (error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult (CancellationToken _) => Cancel;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(_error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<CancellationToken, UniTask> success,
            Func<CancellationToken, UniTask> cancellation,
            Func<Exception, CancellationToken, UniTask> failure,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                return success.Invoke(token);
            }
            else if (IsCancellation)
            {
                return cancellation.Invoke(token);
            }
            else
            {
                return failure.Invoke(_error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult Run(Action whenSuccessful)
        {
            if (IsSuccessful) whenSuccessful.Invoke();

            return this;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult Combine(AsyncResult another)
        {
            var success = IsSuccessful & another.IsSuccessful;
            var cancellation = IsCancellation & another.IsCancellation;

            if (success)
            {
                return Success;
            }
            else if (cancellation)
            {
                return Cancel;
            }
            else
            {
                return Failure;
            }
        }
    }
}
