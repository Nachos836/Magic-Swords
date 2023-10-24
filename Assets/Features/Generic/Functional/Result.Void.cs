using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct Result
    {
        private readonly (bool Provided, Exception Value) _error;

        private Result(Exception error) => _error = (Provided: true, error);

        public static Result Success { get; } = new ();
        public static Result Error { get; } = new (Outcome.Unexpected.Error);

        public bool IsSuccessful => _error.Provided is false;
        public bool IsFailure => _error.Provided;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Result (Exception error) => new (error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result FromException(Exception exception) => exception;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TMatch> success, Func<Exception, TMatch> error)
        {
            return IsSuccessful
                ? success.Invoke()
                : error.Invoke(_error.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action success, Action<Exception> error)
        {
            if (IsSuccessful)
            {
                success.Invoke();
            }
            else
            {
                error.Invoke(_error.Value);
            }
        }
    }
}
