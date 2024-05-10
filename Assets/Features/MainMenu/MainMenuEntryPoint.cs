using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace MagicSwords.Features.MainMenu
{
    using Logger;
    using SceneOperations;

    internal sealed class MainMenuEntryPoint : IAsyncStartable
    {
        private readonly ILogger _logger;
        private readonly PlayerLoopTiming _initializationPoint;

        public MainMenuEntryPoint
        (
            ILogger logger,
            PlayerLoopTiming initializationPoint
        ) {
            _logger = logger;
            _initializationPoint = initializationPoint;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Вот наше главное меню!");

            // InnerHandlerAsync(_sceneLoader, _logger, cancellation).Forget();

            return;

            // static async UniTaskVoid InnerHandlerAsync
            // (
            //     LoadingJob loadingJob,
            //     ILogger logger,
            //     CancellationToken cancellation = default
            // ) {
            //     var result = await loadingJob.Invoke(cancellation);
            //
            //     result.Match
            //     (
            //         success: _ => logger.LogInformation("Игра началась!"),
            //         cancellation: _ => logger.LogWarning("Начало игры было отмененоё"),
            //         error: (exception, _) => logger.LogException(exception),
            //         cancellation
            //     );
            // }
        }
    }
}
