using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.MainMenu
{
    using Logger;

    internal sealed class MainMenuEntryPoint : IAsyncStartable
    {
        private readonly ILogger _logger;

        public MainMenuEntryPoint(ILogger logger)
        {
            _logger = logger;
        }

        public UniTask StartAsync(CancellationToken cancellation)
        {
            _logger.LogInformation("Вот наше главное меню!");

            return UniTask.CompletedTask;
        }
    }
}
