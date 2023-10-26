using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct Optional<TValue>
    {
        private readonly TValue? _value;
        private readonly bool _hasSome;

        private Optional(TValue? value)
        {
            _value = value;

            _hasSome = _value is not null;
        }

        public static Optional<TValue> None { get; } = new ();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<TValue> Some(TValue? value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Optional<TValue> (TValue value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TValue, TAnother> Attach<TAnother>(TAnother? another)
        {
            return _hasSome
                ? Optional<TValue, TAnother>.Some(_value, another)
                : Optional<TValue, TAnother>.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Action<TValue> transformation)
        {
            if (_hasSome) transformation.Invoke(_value!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TValue> Run(Func<TValue, TValue> transformation)
        {
            return _hasSome
                ? Some(transformation.Invoke(_value!))
                : this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action<TValue> some, Action none)
        {
            if (_hasSome)
            {
                some.Invoke(_value!);
            }
            else
            {
                none.Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TValue, TMatch> some, Func<TMatch> none)
        {
            return _hasSome
                ? some.Invoke(_value!)
                : none.Invoke();
        }
    }

    [BurstCompile]
    public readonly struct Optional<TFirst, TSecond>
    {
        private readonly TFirst? _first;
        private readonly TSecond? _second;
        private readonly bool _hasSome;

        private Optional(TFirst? first, TSecond? second)
        {
            (_first, _second) = (first, second);

            _hasSome = _first is not null & _second is not null;
        }

        public static Optional<TFirst, TSecond> None { get; } = new ();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<TFirst, TSecond> Some(TFirst? value, TSecond? second) => new (value, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Optional<TFirst, TSecond>((TFirst? First, TSecond? Second) value)
        {
            return new Optional<TFirst, TSecond>(value.First, value.Second);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Action<TFirst, TSecond> transformation)
        {
            if (_hasSome) transformation.Invoke(_first!, _second!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TFirst, TSecond> Run(Func<TFirst, TSecond, (TFirst First, TSecond Second)> transformation)
        {
            return _hasSome
                ? transformation.Invoke(_first!, _second!)
                : this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncResult<TAnother> Run<TAnother>
        (
            Func<TFirst, TSecond, CancellationToken, AsyncResult<TAnother>> whenSome,
            Func<CancellationToken, AsyncResult<TAnother>> whenNone,
            CancellationToken cancellation
        ) {
            return _hasSome
                ? whenSome.Invoke(_first!, _second!, cancellation)
                : whenNone.Invoke(cancellation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action<TFirst, TSecond> some, Action none)
        {
            if (_hasSome)
            {
                some.Invoke(_first!, _second!);
            }
            else
            {
                none.Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<TFirst, TSecond, TMatch> some, Func<TMatch> none)
        {
            return _hasSome
                ? some.Invoke(_first!, _second!)
                : none.Invoke();
        }
    }
}
