using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZBase.Foundation.Mvvm.ComponentModel;

namespace MagicSwords.Features.MainMenu
{
    using Generic.Functional;

    internal sealed class MainMenuModel
    {
        private readonly Func<CancellationToken, UniTask<AsyncResult>> _sceneLoader;

        public MainMenuModel(Func<CancellationToken, UniTask<AsyncResult>> sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void ApplicationExitHandler(in PropertyChangeEventArgs args)
        {
#       if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#       else
            UnityEngine.Application.Quit();
#       endif
        }

        public void ApplicationRestartHandler(in PropertyChangeEventArgs args)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void StartGameHandler(in PropertyChangeEventArgs args)
        {
            InnerHandlerAsync(_sceneLoader, Application.exitCancellationToken).Forget();

            return;

            static async UniTaskVoid InnerHandlerAsync
            (
                Func<CancellationToken, UniTask<AsyncResult>> loadingJob,
                CancellationToken cancellation = default
            ) {
                var result = await loadingJob.Invoke(cancellation);

                await result.MatchAsync
                (
                    success: async token => { return 5; },
                    cancellation: async token => { return 0; },
                    failure: async (exception, token) => { return 6; },
                    cancellation
                );
            }
        }
    }
}
