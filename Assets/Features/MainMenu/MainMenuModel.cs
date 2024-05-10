using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.SceneOperations;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    using Generic.Functional;
    using Logger;
    using ApplicationExit;

    internal sealed class MainMenuModel
    {
        private readonly IApplicationExitRoutine _exitRoutine;
        private readonly LoadingJob _sceneLoader;
        private readonly ILogger _logger;

        public MainMenuModel
        (
            IApplicationExitRoutine exitRoutine,
            LoadingJob sceneLoader,
            ILogger logger
        ) {
            _exitRoutine = exitRoutine;
            _sceneLoader = sceneLoader;
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
                LoadingJob sceneLoader,
                ILogger logger,
                CancellationToken cancellation = default
            ) {
                var result = await sceneLoader.Invoke(cancellation);

                await result.MatchAsync
                (
                    success: async (sceneHandler, token) =>
                    {
                        if (token.IsCancellationRequested) return;

                        logger.LogInformation("Игра началась!");

                        // await UniTask.Delay(TimeSpan.FromSeconds(3), DelayType.Realtime, PlayerLoopTiming.Update, token, cancelImmediately: true)
                        //     .SuppressCancellationThrow();
                        //
                        // await sceneHandler;

                        logger.LogInformation("Сцена с игрой выгружена!");
                    },
                    cancellation: _ =>
                    {
                        logger.LogWarning("Начало игры было отмененоё");

                        return UniTask.CompletedTask;
                    },
                    error: (exception, _) =>
                    {
                        logger.LogException(exception);

                        return UniTask.CompletedTask;
                    },
                    cancellation
                );
            }
        }
    }
}
