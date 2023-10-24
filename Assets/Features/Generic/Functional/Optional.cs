using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct Optional<T>
    {
        private readonly T? _value;

        private Optional(T? value) => _value = value;

        public static Optional<T> None { get; } = new ();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Some(T? value) => new (value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Action<T> transformation)
        {
            if (_value is not null) transformation.Invoke(_value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<T> Run(Func<T, T> transformation)
        {
            return _value is not null
                ? Some(transformation.Invoke(_value))
                : this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Match(Action<T> some, Action none)
        {
            if (_value is not null)
            {
                some.Invoke(_value);
            }
            else
            {
                none.Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TMatch Match<TMatch>(Func<T, TMatch> some, Func<TMatch> none)
        {
            return _value is not null
                ? some.Invoke(_value)
                : none.Invoke();
        }
    }
}
