using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.ApplicationEntry
{
    using Generic.Functional;
    using Logger;

    internal sealed class ApplicationEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly Func<CancellationToken, UniTask<AsyncResult>> _sceneLoader;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly ILogger _logger;

        public ApplicationEntryPoint
        (
            Func<CancellationToken, UniTask<AsyncResult>> sceneLoader,
            PlayerLoopTiming initializationPoint,
            ILogger logger
        ) {
            _sceneLoader = sceneLoader;
            _initializationPoint = initializationPoint;
            _logger = logger;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Ура, мы начали проект!!!");

            var loading = await _sceneLoader.Invoke(cancellation);
            await loading.MatchAsync
            (
                success: _ =>
                {
                    _logger.LogInformation("Мы загрузились!!");

                    return UniTask.CompletedTask;
                },
                cancellation: _ =>
                {
                    _logger.LogWarning("Загрузка прервана!!");

                    return UniTask.CompletedTask;
                },
                failure: (exception, _) =>
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
