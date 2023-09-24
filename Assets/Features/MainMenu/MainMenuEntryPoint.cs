using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.MainMenu
{
    using Logger;

    internal sealed class MainMenuEntryPoint : IAsyncStartable
    {
        private readonly ILogger _logger;
        private readonly PlayerLoopTiming _initializationPoint;

        public MainMenuEntryPoint(ILogger logger, PlayerLoopTiming initializationPoint)
        {
            _logger = logger;
            _initializationPoint = initializationPoint;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_initializationPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Вот наше главное меню!");
        }
    }
}
