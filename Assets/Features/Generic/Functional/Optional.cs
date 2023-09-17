using System;
using JetBrains.Annotations;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Functional
{
    [BurstCompile]
    public readonly struct Optional<T>
    {
        [CanBeNull] private readonly T _value;

        public Optional(T value) => _value = value;

        public static Optional<T> None { get; }  = new();
        public static Optional<T> Some(T value) => new (value);

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

        public Optional<T> Run(Func<T, T> transformation)
        {
            return _value is not null
                ? new Optional<T>(transformation.Invoke(_value))
                : None;
        }

        public void Run(Action<T> transformation)
        {
            if (_value is not null) transformation.Invoke(_value);
        }
    }
}
