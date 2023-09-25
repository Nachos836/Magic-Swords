using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    using Generic.Functional;
    using ApplicationExit;

    internal sealed class MainMenuModel
    {
        private readonly Func<CancellationToken, UniTask<AsyncResult>> _sceneLoader;
        private readonly IApplicationExitRoutine _exitRoutine;

        public MainMenuModel
        (
            Func<CancellationToken, UniTask<AsyncResult>> sceneLoader,
            IApplicationExitRoutine exitRoutine
        ) {
            _sceneLoader = sceneLoader;
            _exitRoutine = exitRoutine;
        }

        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
        {
            _exitRoutine.Perform();
        }

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
        }

        public void StartGameHandler(in PropertyChangeEventArgs args)
        {
            InnerHandlerAsync(_sceneLoader, _exitRoutine.CancellationToken).Forget();

            return;

            static async UniTaskVoid InnerHandlerAsync
            (
                Func<CancellationToken, UniTask<AsyncResult>> loadingJob,
                CancellationToken cancellation = default
            ) {
                var result = await loadingJob.Invoke(cancellation);

                await result.MatchAsync
                (
                    success: _ => UniTask.FromResult(AsyncResult.Success),
                    cancellation: _ => UniTask.FromResult(AsyncResult.Cancel),
                    failure: (_, _) => UniTask.FromResult(AsyncResult.Failure),
                    cancellation
                );
            }
        }
    }
}
