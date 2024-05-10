using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.SceneOperations;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace MagicSwords.Features.Dialog
{
    using Text.UI;
    using Logger;

    internal sealed class DialogEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly ILogger _logger;
        private readonly PlayerLoopTiming _initializationPoint;
        private readonly ITextPanel _panel;
        private readonly IBufferedAsyncSubscriber<LoadingJob> _mainMenuUnLoader;

        private IDisposable _unLoader = NothingToUnload.Instance;

        public DialogEntryPoint
        (
            ILogger logger,
            PlayerLoopTiming initializationPoint,
            ITextPanel panel,
            IBufferedAsyncSubscriber<LoadingJob> mainMenuUnLoader
        ) {
            _logger = logger;
            _initializationPoint = initializationPoint;
            _panel = panel;
            _mainMenuUnLoader = mainMenuUnLoader;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Вот начало диалога!");

            var outcome = await _panel.LoadAsync(cancellation);
            _unLoader = await outcome.MatchAsync
            (
                success: async (scopeActivator, token) =>
                {
                    if (token.IsCancellationRequested) return NothingToUnload.Instance;

                    var subscription = await _mainMenuUnLoader.SubscribeAsync(static async (handler, token) =>
                    {
                        await handler.Invoke(token);
                    }, cancellationToken: token);

                    return DisposableBag.Create(subscription, scopeActivator.Activate());
                    // return scopeActivator.Activate();
                },
                cancellation: _ =>
                {
                    _logger.LogWarning("Выполнение было отменено!");

                    return UniTask.FromResult(NothingToUnload.Instance);
                },
                error: (exception, _) =>
                {
                    _logger.LogException(exception);

                    return UniTask.FromResult(NothingToUnload.Instance);
                },
                cancellation
            );
        }

        void IDisposable.Dispose() => _unLoader.Dispose();

        private sealed class NothingToUnload : IDisposable
        {
            internal static IDisposable Instance { get; } = new NothingToUnload();
            private NothingToUnload() { }
            public void Dispose() { }
        }
    }
}
