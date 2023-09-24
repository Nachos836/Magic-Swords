using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace MagicSwords.Features.ApplicationEntry
{
    using Generic.Functional;

    internal sealed class ApplicationEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly Func<CancellationToken, UniTask<AsyncResult>> _sceneLoader;

        public ApplicationEntryPoint(Func<CancellationToken, UniTask<AsyncResult>> sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Ура, мы начали проект!!!");

            var loading = await _sceneLoader.Invoke(cancellation);
            await loading.MatchAsync
            (
                success: _ =>
                {
                    Debug.Log("Мы загрузились!!");

                    return UniTask.CompletedTask;
                },
                cancellation: _ =>
                {
                    Debug.Log("Загрузка прервана!!");

                    return UniTask.CompletedTask;
                },
                failure: (exception, _) =>
                {
                    Debug.Log(exception);

                    return UniTask.CompletedTask;
                },
                cancellation
            );
        }

        void IDisposable.Dispose()
        {
            Debug.Log("Пока!!");
        }
    }
}
