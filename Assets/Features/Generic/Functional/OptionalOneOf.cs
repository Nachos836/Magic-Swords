using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    internal readonly struct OptionalOneOf<TFirst, TSecond>
    {
        private readonly (TFirst Value, bool Provided) _first;
        private readonly (TSecond Value, bool Provided) _second;

        private OptionalOneOf(TFirst value)
        {
            _first = (value, Provided: true);
            _second = default;
        }

        private OptionalOneOf(TSecond value)
        {
            _first = default;
            _second = (value, Provided: true);
        }

        public static OptionalOneOf<TFirst, TSecond> None { get; } = new ();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalOneOf<TFirst, TSecond> (TFirst value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalOneOf<TFirst, TSecond> (TSecond value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalOneOf<TFirst, TSecond> From(TFirst value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OptionalOneOf<TFirst, TSecond> From(TSecond value) => value;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TFirst, TMatch> whenFirst,
            Func<TSecond, TMatch> whenSecond,
            Func<TMatch> whenNone
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
                return whenNone.Invoke();
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<TFirst> whenFirst,
            Action<TSecond> whenSecond,
            Action whenNone
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
                whenNone.Invoke();
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, CancellationToken, UniTask<TMatch>> whenFirst,
            Func<TSecond, CancellationToken, UniTask<TMatch>> whenSecond,
            Func<CancellationToken, UniTask<TMatch>> whenNone,
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
                return whenNone.Invoke(cancellation);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TFirst, CancellationToken, UniTask> whenFirst,
            Func<TSecond, CancellationToken, UniTask> whenSecond,
            Func<CancellationToken, UniTask> whenNone,
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
                return whenNone.Invoke(cancellation);
            }
        }
    }
}
