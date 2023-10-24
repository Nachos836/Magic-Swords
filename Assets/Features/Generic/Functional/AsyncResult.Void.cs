using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct AsyncResult
    {
        private readonly (CancellationToken Value, bool Provided) _cancellation;
        private readonly (Exception Value, bool Provided) _exception;

        private AsyncResult(bool success = true)
        {
            IsSuccessful = success;

            _cancellation = default;
            _exception = default;
        }

        private AsyncResult(CancellationToken cancellation)
        {
            IsSuccessful = false;

            _cancellation = (cancellation, Provided: true);
            _exception = default;
        }

        private AsyncResult(Exception exception)
        {
            IsSuccessful = false;

            _cancellation = default;
            _exception = (exception, Provided: true);
        }

        public static AsyncResult Success { get; } = new (success: true);
        public static AsyncResult Cancel { get; } = new (CancellationToken.None);
        public static AsyncResult Error { get; } = new (Unexpected.Error);
        public static AsyncResult Impossible { get; } = new (Unexpected.Impossible);

        public bool IsSuccessful { get; }
        public bool IsCancellation => _cancellation.Provided;
        public bool IsError => _exception.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncResult (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult Combine(AsyncResult another)
        {
            var success = IsSuccessful & another.IsSuccessful;
            var cancellation = IsCancellation | another.IsCancellation;

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
                if (IsError) return this;
                if (another.IsError) return another;

                return Impossible;
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
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
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
                return error.Invoke(_exception.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<CancellationToken, UniTask> success,
            Func<CancellationToken, UniTask> cancellation,
            Func<Exception, CancellationToken, UniTask> error,
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
                return error.Invoke(_exception.Value, token);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<CancellationToken> success,
            Action<CancellationToken> cancellation,
            Action<Exception, CancellationToken> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                success.Invoke(token);
            }
            else if (IsCancellation)
            {
                cancellation.Invoke(token);
            }
            else
            {
                error.Invoke(_exception.Value, token);
            }
        }
    }
}
