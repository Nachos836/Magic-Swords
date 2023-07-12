using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace MagicSwords.Features.Generic.StateMachine
{
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    internal sealed class StateMachine
    {
        private readonly ConcurrentDictionary<int, (IState From, IState To)> _transitions = new ();

        private IState _current;

        public StateMachine()
        {
            var candidate = _transitions.FirstOrDefault();

            _current = candidate.Equals(default)
                ? candidate.Value.From
                : new InitialState();
        }

        public async UniTask TransitAsync<TTrigger>(CancellationToken cancellation = default)
        {
            if (_transitions.TryGetValue(UniqueId<TTrigger>.Value, out var transit))
            {
                if (_current == transit.From)
                {
                    if (_current is IState.IWithExitAction exit) await exit.OnExitAsync(cancellation);

                    _current = transit.To;

                    if (_current is IState.IWithEnterAction enter) await enter.OnEnterAsync(cancellation);

                    if (_current is IState.IWithUpdateLoop update) await foreach
                    (
                        var _ in UniTaskAsyncEnumerable
                            .EveryUpdate(update.LoopTiming)
                            .WithCancellation(cancellation)
                    ) {
                        await update.OnUpdate(cancellation);
                    }
                }
            }
        }

        public StateMachine AddTransition<TTrigger>(IState from, IState to)
        {
            if (_transitions.ContainsKey(UniqueId<TTrigger>.Value)) throw new ArgumentException($"Transition {typeof(TTrigger).Name} are already added!");

            if (_transitions.TryAdd(UniqueId<TTrigger>.Value, (from, to))) return this;

            throw new ArgumentException($"Impossible to add Transition {typeof(TTrigger).Name}");
        }

        private static class UniqueNumberHolder
        {
            public static int Value;
        }

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        private static class UniqueId<T>
        {
            public static int Value { get; } = UniqueNumberHolder.Value++;
        }
    }
}