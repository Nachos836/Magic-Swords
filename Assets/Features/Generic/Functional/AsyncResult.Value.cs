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
    public readonly struct AsyncResult<TValue>
    {
        private readonly (Exception Value, bool Provided) _error;
        private readonly (Cancellation Value, bool Provided) _cancellation;
        private readonly (TValue Value, bool Provided) _income;

        public AsyncResult(TValue value)
        {
            _income = (value, true);
            _error = default;
            _cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            _income = default;
            _error = (error, Provided: true);
            _cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            _income = default;
            _error = default;
            _cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult<TValue> Failure { get; } = new (Error);
        public static AsyncResult<TValue> Cancel { get; } = new (Canceled);

        public bool IsSuccessful => _income is { Provided: true };
        public bool IsFailure => _error is { Provided: true };
        public bool IsCancellation => _cancellation is { Provided: true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (TValue value) => new (value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (Exception error) => new (error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult<TValue> (CancellationToken _) => Cancel;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> failure,
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
                return failure.Invoke(_error.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMap> MapAsync<TMap>
        (
            Func<TValue, CancellationToken, UniTask<TMap>> success,
            Func<CancellationToken, UniTask<TMap>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMap>> failure,
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
                return failure.Invoke(_error.Value, token);
            }
        }
    }
}
