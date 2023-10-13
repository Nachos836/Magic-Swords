using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        private IDisposable _unLoader;

        public DialogEntryPoint
        (
            ILogger logger,
            PlayerLoopTiming initializationPoint,
            ITextPanel panel
        ) {
            _logger = logger;
            _initializationPoint = initializationPoint;
            _panel = panel;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Вот начало диалога!");

            var outcome = await _panel.LoadAsync(cancellation);
            _unLoader = await outcome.MapAsync
            (
                success: static (unLoader, _) => UniTask.FromResult(unLoader),
                cancellation: _ =>
                {
                    _logger.LogWarning("Выполнение было отменено!");

                    return UniTask.FromResult(NothingToUnload.Instance);
                },
                failure: (exception, _) =>
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
            private NothingToUnload() { }
            internal static IDisposable Instance { get; } = new NothingToUnload();
            public void Dispose() { }
        }
    }
}
