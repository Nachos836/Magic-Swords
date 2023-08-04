using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private readonly bool _success;

        private AsyncResult(NoneResult value = default)
        {
            _success = true;
            _error = default;
            _cancellation = default;
        }

        public AsyncResult(Exception error)
        {
            _success = false;
            _error = (error, Provided: true);
            _cancellation = default;
        }

        private AsyncResult(Cancellation cancellation)
        {
            _success = false;
            _error = default;
            _cancellation = (cancellation, Provided: true);
        }

        public static AsyncResult Success { get; } = new (value: default);
        public static AsyncResult Failure { get; } = new (Error);
        public static AsyncResult Cancel { get; } = new (Canceled);

        public bool IsSuccessful => _success;
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
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneResult { }
    }
}
