using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.Generic.StateMachine;
using UnityEngine;

namespace MagicSwords.Features.Dialog
{
    internal sealed class StateMachineTestComponent : MonoBehaviour
    {
        private readonly StateMachine<int> _stateMachine = new ();

        private async UniTaskVoid Start()
        {
            _stateMachine.AddState(() => new State1());
            _stateMachine.AddState(() => new State2());

            _stateMachine.AddTransition<InitialState, State1>(0);
            _stateMachine.AddTransition<State1, State2>(69);
            _stateMachine.AddTransition<State2, State1>(96);

            await _stateMachine.TransitAsync(0, destroyCancellationToken);
            await _stateMachine.TransitAsync(69, destroyCancellationToken);
            await _stateMachine.TransitAsync(96, destroyCancellationToken);
        }
    }

    internal sealed class State1 : IState, IState.IWithEnterAction
    {
        UniTask IState.IWithEnterAction.OnEnterAsync(CancellationToken cancellation)
        {
            Debug.Log($"Привет, мы вошли в {nameof(State1)}");

            return UniTask.CompletedTask;
        }
    }

    internal sealed class State2 : IState, IState.IWithExitAction
    {
        UniTask IState.IWithExitAction.OnExitAsync(CancellationToken cancellation)
        {
            Debug.Log($"Пока, мы покидаем {nameof(State2)}");

            return UniTask.CompletedTask;
        }
    }
}