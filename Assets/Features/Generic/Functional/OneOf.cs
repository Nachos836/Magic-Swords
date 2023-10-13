using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    internal readonly struct OneOf<TFirst, TSecond>
    {
        private readonly (TFirst Value, bool Provided) _first;
        private readonly (TSecond Value, bool Provided) _second;

        public OneOf(TFirst value)
        {
            _first = (value, Provided: true);
            _second = default;
        }

        public OneOf(TSecond value)
        {
            _first = default;
            _second = (value, Provided: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OneOf<TFirst, TSecond> (TFirst value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OneOf<TFirst, TSecond> (TSecond value) => new (value);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TFirst, TMatch> whenFirst,
            Func<TSecond, TMatch> whenSecond
        ) {
            return _first is { Provided: true }
                ? whenFirst.Invoke(_first.Value)
                : whenSecond.Invoke(_second.Value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<TFirst> whenFirst,
            Action<TSecond> whenSecond
        ) {
            if (_first is { Provided: true })
            {
                whenFirst.Invoke(_first.Value);
            }
            else
            {
                whenSecond.Invoke(_second.Value);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, CancellationToken, UniTask<TMatch>> whenFirst,
            Func<TSecond, CancellationToken, UniTask<TMatch>> whenSecond,
            CancellationToken cancellation = default
        ) {
            return _first is { Provided: true }
                ? whenFirst.Invoke(_first.Value, cancellation)
                : whenSecond.Invoke(_second.Value, cancellation);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TFirst, CancellationToken, UniTask> whenFirst,
            Func<TSecond, CancellationToken, UniTask> whenSecond,
            CancellationToken cancellation = default
        ) {
            return  _first is { Provided: true }
                ? whenFirst.Invoke(_first.Value, cancellation)
                : whenSecond.Invoke(_second.Value, cancellation);
        }
    }

    [BurstCompile]
    public readonly struct OneOf<TFirst, TSecond, TThird>
    {
        private readonly (TFirst Value, bool Provided) _first;
        private readonly (TSecond Value, bool Provided) _second;
        private readonly (TThird Value, bool Provided) _third;

        public OneOf(TFirst value)
        {
            _first = (value, Provided: true);
            _second = default;
            _third = default;
        }

        public OneOf(TSecond value)
        {
            _first = default;
            _second = (value, Provided: true);
            _third = default;
        }

        public OneOf(TThird value)
        {
            _first = default;
            _second = default;
            _third = (value, Provided: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OneOf<TFirst, TSecond, TThird> (TFirst value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OneOf<TFirst, TSecond, TThird> (TSecond value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OneOf<TFirst, TSecond, TThird> (TThird value) => new (value);

        public static OneOf<TFirst, TSecond, TThird> From(TFirst value) => new(value);
        public static OneOf<TFirst, TSecond, TThird> From(TSecond value) => new(value);
        public static OneOf<TFirst, TSecond, TThird> From(TThird value) => new(value);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TFirst, TMatch> whenFirst,
            Func<TSecond, TMatch> whenSecond,
            Func<TThird, TMatch> whenThird
        ) {
            if (_first is { Provided: true } first)
            {
                return whenFirst.Invoke(first.Value);
            }
            else if (_second is { Provided: true } second)
            {
                return whenSecond.Invoke(second.Value);
            }
            else
            {
                return whenThird.Invoke(_third.Value);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<TFirst> whenFirst,
            Action<TSecond> whenSecond,
            Action<TThird> whenThird
        ) {
            if (_first is { Provided: true } first)
            {
                whenFirst.Invoke(first.Value);
            }
            else if (_second is { Provided: true } second)
            {
                whenSecond.Invoke(second.Value);
            }
            else
            {
                whenThird.Invoke(_third.Value);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, CancellationToken, UniTask<TMatch>> whenFirst,
            Func<TSecond, CancellationToken, UniTask<TMatch>> whenSecond,
            Func<TThird, CancellationToken, UniTask<TMatch>> whenThird,
            CancellationToken cancellation = default
        ) {
            if (_first is { Provided: true } first)
            {
                return whenFirst.Invoke(first.Value, cancellation);
            }
            else if (_second is { Provided: true } second)
            {
                return whenSecond.Invoke(second.Value, cancellation);
            }
            else
            {
                return whenThird.Invoke(_third.Value, cancellation);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TFirst, CancellationToken, UniTask> whenFirst,
            Func<TSecond, CancellationToken, UniTask> whenSecond,
            Func<TThird, CancellationToken, UniTask> whenThird,
            CancellationToken cancellation = default
        ) {
            if (_first is { Provided: true } first)
            {
                return whenFirst.Invoke(first.Value, cancellation);
            }
            else if (_second is { Provided: true } second)
            {
                return whenSecond.Invoke(second.Value, cancellation);
            }
            else
            {
                return whenThird.Invoke(_third.Value, cancellation);
            }
        }
    }
}
