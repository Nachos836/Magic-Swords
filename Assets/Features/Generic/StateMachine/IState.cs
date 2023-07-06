using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.StateMachine
{
    internal interface IState
    {
        internal interface IEnterable
        {
            UniTask OnEnterAsync(CancellationToken cancellation = default);
        }

        internal interface IExitable
        {
            UniTask OnExitAsync(CancellationToken cancellation = default);
        }
    }

    internal sealed class InitialState : IState
    {
        
    }
}