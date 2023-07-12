using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        public OptionalOneOf(TFirst value)
        {
            _first = (value, Provided: true);
            _second = default;
        }

        public OptionalOneOf(TSecond value)
        {
            _first = default;
            _second = (value, Provided: true);
        }

        private OptionalOneOf(NoneOf noneOf = default)
        {
            _first = default;
            _second = default;
        }

        public static OptionalOneOf<TFirst, TSecond> None { get; } = new (noneOf: default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalOneOf<TFirst, TSecond> (TFirst value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator OptionalOneOf<TFirst, TSecond> (TSecond value) => new (value);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>
        (
            Func<TFirst, TMatch> whenFirst,
            Func<TSecond, TMatch> whenSecond,
            Func<TMatch> none
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
                return none.Invoke();
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match
        (
            Action<TFirst> whenFirst,
            Action<TSecond> whenSecond,
            Action none
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
                none.Invoke();
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<TMatch> MatchAsync<TMatch>
        (
            Func<TFirst, CancellationToken, UniTask<TMatch>> whenFirst,
            Func<TSecond, CancellationToken, UniTask<TMatch>> whenSecond,
            Func<CancellationToken, UniTask<TMatch>> none,
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
                return none.Invoke(cancellation);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask MatchAsync
        (
            Func<TFirst, CancellationToken, UniTask> whenFirst,
            Func<TSecond, CancellationToken, UniTask> whenSecond,
            Func<CancellationToken, UniTask> none,
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
                return none.Invoke(cancellation);
            }
        }

        [BurstCompile]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly ref struct NoneOf { }
    }
}
