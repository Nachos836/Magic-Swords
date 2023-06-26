using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features;
using VContainer.Unity;

namespace MagicSwords.DI
{
    internal sealed class EntryPoint : IAsyncStartable, IDisposable
    {
        private readonly ISceneSwitcher _sceneSwitcher;
        private readonly int _mainMenu;

        public EntryPoint(ISceneSwitcher sceneSwitcher, int mainMenu)
        {
            _sceneSwitcher = sceneSwitcher;
            _mainMenu = mainMenu;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Ура, мы начали проект!!!");
            
            await _sceneSwitcher.SwitchAsync(_mainMenu, cancellation);
        }

        void IDisposable.Dispose()
        {
            UnityEngine.Debug.Log("Пока!!");
        }
    }
}