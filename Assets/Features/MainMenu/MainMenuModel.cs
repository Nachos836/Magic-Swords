using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    using Generic.Functional;
    using Logger;
    using ApplicationExit;

    internal sealed class MainMenuModel
    {
        private readonly Func<CancellationToken, UniTask<AsyncResult>> _sceneLoader;
        private readonly IApplicationExitRoutine _exitRoutine;
        private readonly ILogger _logger;

        public MainMenuModel
        (
            Func<CancellationToken, UniTask<AsyncResult>> sceneLoader,
            IApplicationExitRoutine exitRoutine,
            ILogger logger
        ) {
            _sceneLoader = sceneLoader;
            _exitRoutine = exitRoutine;
            _logger = logger;
        }

        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
        {
            _exitRoutine.Perform();
        }

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
        }

        public void StartGameHandler(in PropertyChangeEventArgs _, CancellationToken cancellation)
        {
            InnerHandlerAsync(_sceneLoader, _logger, cancellation).Forget();

            return;

            static async UniTaskVoid InnerHandlerAsync
            (
                Func<CancellationToken, UniTask<AsyncResult>> loadingJob,
                ILogger logger,
                CancellationToken cancellation = default
            ) {
                var result = await loadingJob.Invoke(cancellation);

                result.Match
                (
                    success: _ => logger.LogInformation("Игра началась!"),
                    cancellation: _ => logger.LogWarning("Начало игры было отмененоё"),
                    error: (exception, _) => logger.LogException(exception),
                    cancellation
                );
            }
        }
    }
}
