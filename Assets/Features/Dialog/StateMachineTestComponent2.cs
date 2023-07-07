using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MagicSwords.Features.Dialog
{
    using Generic.StateMachine;

    internal sealed class StateMachineTestComponent2 : MonoBehaviour
    {
        private readonly StateMachine2 _stateMachine = new ();

        private async UniTaskVoid Start()
        {
            _stateMachine.AddTransition<Button>(new State1(), new State2());

            await _stateMachine.TransitAsync<Button>(destroyCancellationToken);
        }
    }
}