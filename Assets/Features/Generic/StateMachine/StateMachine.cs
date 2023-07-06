using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace MagicSwords.Features.Generic.StateMachine
{
    internal delegate TState StateFactory<out TState>() where TState : IState;
    
    internal class StateMachine<TTrigger>
    {
        private readonly Dictionary<Type, IStateDefinition> _states = new()
        {
            {typeof(InitialState), new StateDefinition<InitialState>(() => new InitialState())}
        };
        private readonly Dictionary<(Type, TTrigger), IStateDefinition> _transitions = new ();

        private (IStateDefinition Definition, IState State) _activeState;

        public StateMachine()
        {
            _activeState = (_states.First().Value, new InitialState());
        }

        public async UniTask TransitAsync(TTrigger trigger, CancellationToken cancellation = default)
        {
            var activeDefinition = _activeState.Definition;
            var transitionDefinition = (activeDefinition.Type, trigger);

            Assert.IsTrue
            (
                _transitions.ContainsKey(transitionDefinition),
                $"Transition from state {_activeState.State?.ToString() ?? "ROOT"} not found by trigger { trigger }"
            );

            activeDefinition = _transitions[transitionDefinition];

            if (_activeState.State is IState.IWithExitAction exit)
            {
                await exit.OnExitAsync(cancellation);
            }

            _activeState = (activeDefinition, activeDefinition?.CreateState());

            if (_activeState.State is IState.IWithEnterAction enter)
            {
                await enter.OnEnterAsync(cancellation);
            }
        }

        public void AddState<TState>(StateFactory<TState> factory) where TState : IState
        {
            _states[typeof(TState)] = new StateDefinition<TState>(factory);
        }

        public void AddTransition<TState1, TState2>(TTrigger trigger)
            where TState1 : IState
            where TState2 : IState
        {
            var type1 = typeof(TState1);
            var type2 = typeof(TState2);

            Assert.IsTrue(_states.ContainsKey(type1), $"State of type { type1 } is not defined");
            Assert.IsTrue(_states.ContainsKey(type2), $"State of type { type2 } is not defined");

            _transitions[(type1, trigger)] = _states[type2];
        }

        public void AddTransitionToInitial<TState>(TTrigger trigger) where TState : IState
        {
            var type = typeof(TState);

            Assert.IsTrue(_states.ContainsKey(type), $"State of type { type } is not defined");

            _transitions[(null, trigger)] = _states[type];
        }

        private interface IStateDefinition
        {
            Type Type { get; }
            IState CreateState();
        }

        private sealed class StateDefinition<TState> : IStateDefinition where TState : IState
        {
            private readonly StateFactory<TState> _factory;

            public Type Type => typeof(TState);

            public StateDefinition(StateFactory<TState> factory)
            {
                _factory = factory;
            }

            public IState CreateState()
            {
                return _factory.Invoke();
            }
        }
    }
}