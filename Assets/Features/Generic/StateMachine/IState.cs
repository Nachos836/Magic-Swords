using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.StateMachine
{
    internal interface IState
    {
        internal interface IWithEnterAction
        {
            UniTask OnEnterAsync(CancellationToken cancellation = default);
        }

        internal interface IWithExitAction
        {
            UniTask OnExitAsync(CancellationToken cancellation = default);
        }
    }

    internal sealed class InitialState : IState { }
}