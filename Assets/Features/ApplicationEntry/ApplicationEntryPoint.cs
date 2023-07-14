using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.ApplicationEntry
{
    using SceneLoader;

    public sealed class ApplicationEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly int _mainMenu;

        public ApplicationEntryPoint(ISceneLoader sceneLoader, int mainMenu)
        {
            _sceneLoader = sceneLoader;
            _mainMenu = mainMenu;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Ура, мы начали проект!!!");

            await _sceneLoader.LoadAlongsideAsync(_mainMenu, cancellation);
        }

        void IDisposable.Dispose()
        {
            UnityEngine.Debug.Log("Пока!!");
        }
    }
}
