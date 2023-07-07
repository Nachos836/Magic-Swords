using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.StateMachine
{
    internal sealed class StateMachine2
    {
        private readonly ConcurrentDictionary<int, (IState From, IState To)> _transitions = new ();

        private IState _current = new InitialState();

        public async UniTask TransitAsync<TTrigger>(CancellationToken cancellation = default)
        {
            if (_transitions.TryGetValue(UniqueId<TTrigger>.Value, out var transit))
            {
                if (_current == transit.From)
                {
                    if (_current is IState.IWithExitAction exit) await exit.OnExitAsync(cancellation);

                    _current = transit.To;

                    if (_current is IState.IWithEnterAction enter) await enter.OnEnterAsync(cancellation);
                }
            }
        }

        public void AddTransition<TTrigger>(IState from, IState to)
        {
            if (_transitions.TryGetValue(UniqueId<TTrigger>.Value, out _)) return;

            _transitions.AddOrUpdate(UniqueId<TTrigger>.Value, (from, to), (_, tuple) => tuple);
        }

        private static class UniqueNumberHolder
        {
            public static int Value;
        }

        private static class UniqueId<T>
        {
            public static int Value { get; } = UniqueNumberHolder.Value++;
        }
    }
}