using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace MagicSwords.Features.MainMenu
{
    internal sealed class MainMenuEntryPoint : IAsyncStartable
    {
        public UniTask StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Вот наше главное меню!");

            return UniTask.CompletedTask;
        }
    }
}
