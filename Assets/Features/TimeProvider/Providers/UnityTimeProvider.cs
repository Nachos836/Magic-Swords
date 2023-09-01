using UnityEngine;

namespace MagicSwords.Features.TimeProvider.Providers
{
    internal sealed class UnityTimeProvider : ICurrentTimeProvider, IFixedCurrentTimeProvider, IDeltaTimeProvider, IFixedDeltaTimeProvider
    {
        float ICurrentTimeProvider.Value => Time.time;
        float IFixedCurrentTimeProvider.Value => Time.fixedTime;
        float IDeltaTimeProvider.Value => Time.deltaTime;
        float IFixedDeltaTimeProvider.Value => Time.fixedDeltaTime;
    }
}
