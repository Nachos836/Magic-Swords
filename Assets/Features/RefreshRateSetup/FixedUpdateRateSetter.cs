using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace MagicSwords.Features.RefreshRateSetup
{
    using Logger;

    internal sealed class FixedUpdateRateSetter : IAsyncStartable
    {
        private readonly PlayerLoopTiming _startPoint;
        private readonly ILogger _logger;
        private readonly float _framesPerSecond;

        public FixedUpdateRateSetter(float framesPerSecond, PlayerLoopTiming startPoint, ILogger logger)
        {
            _framesPerSecond = framesPerSecond;
            _startPoint = startPoint;
            _logger = logger;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(_startPoint, cancellation)
                .SuppressCancellationThrow()) return;

            _logger.LogInformation("Задана частота фиксированных обновлений {0}", _framesPerSecond);
            UnityEngine.Time.fixedDeltaTime = 1.0f / _framesPerSecond;
        }
    }
}
