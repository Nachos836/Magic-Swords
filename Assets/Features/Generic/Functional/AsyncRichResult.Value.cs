using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    using Outcome;

    [BurstCompile]
    public readonly struct AsyncRichResult<TValue>
    {
        private readonly (TValue Value, bool Provided) _income;
        private readonly (CancellationToken Token, bool Provided) _cancellation;
        private readonly (Expected.Failure Value, bool Provided) _failure;
        private readonly (Exception Value, bool Provided) _exception;

        private AsyncRichResult(TValue value)
        {
            _income = (value, Provided: true);
            _cancellation = default;
            _failure = default;
            _exception = default;
        }

        private AsyncRichResult(CancellationToken cancellation)
        {
            _income = default;
            _cancellation = (cancellation, Provided: true);
            _failure = default;
            _exception = default;
        }

        private AsyncRichResult(Expected.Failure failure)
        {
            _income = default;
            _cancellation = default;
            _failure = (failure, Provided: true);
            _exception = default;
        }

        private AsyncRichResult(Exception exception)
        {
            _income = default;
            _cancellation = default;
            _failure = default;
            _exception = (exception, Provided: true);
        }

        public static AsyncRichResult<TValue> Cancel { get; } = new (CancellationToken.None);
        public static AsyncRichResult<TValue> Failure { get; } = new (Expected.Failed);
        public static AsyncRichResult<TValue> Error { get; } = new (Unexpected.Error);
        public static AsyncRichResult<TValue> Impossible { get; } = new (Unexpected.Impossible);

        public bool IsSuccessful => _income.Provided;
        public bool IsCancellation => _cancellation.Provided;
        public bool IsFailure => _failure.Provided;
        public bool IsError => _exception.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult<TValue> (TValue value) => new (value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult<TValue> (CancellationToken cancellation) => new (cancellation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult<TValue> (Expected.Failure failure) => new (failure);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AsyncRichResult<TValue> (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult<TValue> FromResult(TValue value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult<TValue> FromCancellation(CancellationToken cancellation) => cancellation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult<TValue> FromFailure(Expected.Failure failure) => failure;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncRichResult<TValue> FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TValue> AsResult(Func<Expected.Failure, AsyncResult<TValue>> resolveError)
        {
            if (IsSuccessful)
            {
                return AsyncResult<TValue>.FromResult(_income.Value);
            }
            else if (IsCancellation)
            {
                return AsyncResult<TValue>.FromCancellation(_cancellation.Token);
            }
            else if (IsFailure)
            {
                return resolveError(_failure.Value);
            }
            else
            {
                return AsyncResult<TValue>.FromException(_exception.Value);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncRichResult<TValue> Run(Action<TValue> whenSuccessful)
        {
            if (IsSuccessful) whenSuccessful.Invoke(_income.Value);

            return this;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TValue, CancellationToken, UniTask<TMatch>> success,
            Func<CancellationToken, UniTask<TMatch>> cancellation,
            Func<Expected.Failure, CancellationToken, UniTask<TMatch>> failure,
            Func<Exception, CancellationToken, UniTask<TMatch>> error,
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
            Func<TValue, CancellationToken, UniTask> success,
            Func<CancellationToken, UniTask> cancellation,
            Func<Expected.Failure, CancellationToken, UniTask> failure,
            Func<Exception, CancellationToken, UniTask> error,
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
            Action<TValue, CancellationToken> success,
            Action<CancellationToken> cancellation,
            Action<Expected.Failure, CancellationToken> failure,
            Action<Exception, CancellationToken> error,
            CancellationToken token = default
        ) {
            if (IsSuccessful)
            {
                success.Invoke(_income.Value, token);
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
