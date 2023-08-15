using UnityEngine;

namespace MagicSwords.Features.TextAnimator.TimeProvider
{
    public interface ICurrentTimeProvider
    {
        float Value { get; }
    }

    public interface IFixedCurrentTimeProvider
    {
        float Value { get; }
    }

    public interface IDeltaTimeProvider
    {
        float Value { get; }
    }

    public interface IFixedDeltaTimeProvider
    {
        float Value { get; }
    }

    internal sealed class UnityTimeProvider : ICurrentTimeProvider, IFixedCurrentTimeProvider, IDeltaTimeProvider, IFixedDeltaTimeProvider
    {
        float ICurrentTimeProvider.Value => Time.time;
        float IFixedCurrentTimeProvider.Value => Time.fixedTime;
        float IDeltaTimeProvider.Value => Time.deltaTime;
        float IFixedDeltaTimeProvider.Value => Time.fixedDeltaTime;
    }
}