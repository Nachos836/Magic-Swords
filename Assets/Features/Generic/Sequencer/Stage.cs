using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.Sequencer
{
    using Functional;

    public static class Stage
    {
        private static readonly Lazy<Canceled> LazyCancel= new (static () => new Canceled());
        private static readonly Lazy<Ended> LazyEnd = new (static () => new Ended());
        private static readonly Lazy<Errored> LazyError = new (static () => new Errored());

        public static Canceled Cancel { get; } = LazyCancel.Value;
        public static Ended End { get; } = LazyEnd.Value;
        public static Errored Error { get; } = LazyError.Value;

        public sealed class Errored : IStage { }
        public sealed class Canceled : IStage { }
        public sealed class Ended : IStage { }
    }

    public interface IStage
    {
        public interface IProcess
        {
            UniTask<OneOf<IStage, Stage.Canceled, Stage.Errored>> ProcessAsync(CancellationToken cancellation = default);
        }
    }
}
