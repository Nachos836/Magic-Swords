using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer.Unity;

namespace MagicSwords.Features.ApplicationEntry
{
    using Logger;
    using SceneOperations;

    internal sealed class ApplicationEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly LoadingJob _menuLoader;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly IBufferedAsyncPublisher<LoadingJob> _mainMenuUnLoader;
        private readonly ILogger _logger;

        public ApplicationEntryPoint
        (
            LoadingJob menuLoader,
            PlayerLoopTiming initializationPoint,
            ILogger logger,
            IBufferedAsyncPublisher<LoadingJob> mainMenuUnLoader
        ) {
            _menuLoader = menuLoader;
            _initializationPoint = initializationPoint;
            _logger = logger;
            _mainMenuUnLoader = mainMenuUnLoader;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Ура, мы начали проект!!!");

            var loading = await _menuLoader.Invoke(cancellation);
            await loading.MatchAsync
            (
                success: async (sceneHandler, token) =>
                {
                    if (token.IsCancellationRequested) return;

                    _logger.LogInformation("Мы загрузились!!");

                    await _mainMenuUnLoader.PublishAsync(default!, token)
                        .SuppressCancellationThrow();
                },
                cancellation: _ =>
                {
                    _logger.LogWarning("Загрузка прервана!!");

                    return UniTask.CompletedTask;
                },
                error: (exception, _) =>
                {
                    _logger.LogException(exception);

                    return UniTask.CompletedTask;
                },
                cancellation
            );
        }

        void IDisposable.Dispose()
        {
            _logger.LogInformation("Пока!!");
        }
    }
}
