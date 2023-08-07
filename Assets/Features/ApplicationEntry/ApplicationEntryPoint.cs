using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.ApplicationEntry
{
    using SceneLoader;

    internal sealed class ApplicationEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly ISceneLoader _sceneLoader;

        public ApplicationEntryPoint(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Ура, мы начали проект!!!");

            var loading = await _sceneLoader.LoadAsync(cancellation);
            await loading.MatchAsync
            (
                success: _ =>
                {
                    UnityEngine.Debug.Log("Мы загрузились!!");

                    return UniTask.CompletedTask;
                },
                cancellation: _ =>
                {
                    UnityEngine.Debug.Log("Загрузка прервана!!");

                    return UniTask.CompletedTask;
                },
                failure: (exception, _) =>
                {
                    UnityEngine.Debug.Log(exception);

                    return UniTask.CompletedTask;
                },
                cancellation
            );
        }

        void IDisposable.Dispose()
        {
            UnityEngine.Debug.Log("Пока!!");
        }
    }
}
