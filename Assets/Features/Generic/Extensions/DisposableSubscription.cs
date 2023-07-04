using System;
using MessagePipe;
using Unity.Burst;

namespace MagicSwords.Features.Generic.Extensions
{
    internal static class DisposableSubscription
    {
        public static DisposableBagBuilder Append(this DisposableBagBuilder builder, IDisposable income)
        {
            builder.Add(income);

            return builder;
        }

        [BurstCompile]
        public readonly struct SubscriptionHandler : IDisposable
        {
            private readonly Action _unsubscribe;

            public SubscriptionHandler(Action unsubscribe) => _unsubscribe = unsubscribe;

            void IDisposable.Dispose() => _unsubscribe.Invoke();
        }
    }
}