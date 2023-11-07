using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct AsyncRichResult
    {
        private readonly (CancellationToken Token, bool Provided) _cancellation;
        private readonly (Expected.Failure Value, bool Provided) _failure;
        private readonly (Exception Value, bool Provided) _exception;

        private AsyncRichResult(bool success = true)
        {
            IsSuccessful = success;

            _cancellation = default;
            _failure = default;
            _exception = default;
        }

        private AsyncRichResult(CancellationToken cancellation)
        {
            IsSuccessful = false;

            _cancellation = (cancellation, Provided: true);
            _failure = default;
            _exception = default;
        }

        private AsyncRichResult(Expected.Failure failure)
        {
            IsSuccessful = false;

            _cancellation = default;
            _failure = (failure, Provided: true);
            _exception = default;
        }

        private AsyncRichResult(Exception exception)
        {
            IsSuccessful = false;

            _cancellation = default;
            _failure = default;
            _exception = (exception, Provided: true);
        }

        public static AsyncRichResult Success { get; } = new (success: true);
        public static AsyncRichResult Cancel { get; } = new (CancellationToken.None);
        public static AsyncRichResult Failure { get; } = new (Expected.Failed);
        public static AsyncRichResult Error { get; } = new (Unexpected.Error);
        public static AsyncRichResult Impossible { get; } = new (Unexpected.Impossible);

        public bool IsSuccessful { get; }
        public bool IsCancellation => _cancellation.Provided;
        public bool IsFailure => _failure.Provided;
        public bool IsError => _exception.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult (Expected.Failure failure) => new (failure);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult FromFailure(Expected.Failure failure) => failure;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult AsResult()
        {
            if (IsSuccessful)
            {
                return AsyncResult.Success;
            }
            else if (IsCancellation)
            {
                return AsyncResult.FromCancellation(_cancellation.Token);
            }
            else if (IsError)
            {
                return AsyncResult.FromException(_exception.Value);
            }
            else
            {
                return AsyncResult.FromException(_failure.Value.ToException());
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncRichResult Combine(AsyncRichResult another)
        {
            var success = IsSuccessful & another.IsSuccessful;
            var cancellation = IsCancellation | another.IsCancellation;
            var failure = IsFailure | another.IsFailure;

            if (success)
            {
                return this;
            }
            else if (cancellation)
            {
                return IsCancellation ? this : another;
            }
            else if (failure)
            {
                return IsFailure ? this : another;
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
        public AsyncRichResult Run(Action whenSuccessful)
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
            Func<Expected.Failure, CancellationToken, UniTask<TMatch>> failure,
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
            else if (IsFailure)
            {
                return failure.Invoke(_failure.Value, token);
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
            Func<Expected.Failure, CancellationToken, UniTask> failure,
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
            else if (IsFailure)
            {
                return failure.Invoke(_failure.Value, token);
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
            Action<Expected.Failure, CancellationToken> failure,
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
            else if (IsFailure)
            {
                failure.Invoke(_failure.Value, token);
            }
            else
            {
                error.Invoke(_exception.Value, token);
            }
        }
    }
}
