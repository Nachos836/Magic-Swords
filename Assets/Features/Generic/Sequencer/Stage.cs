using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.Sequencer
{
    using Functional;

    internal static class Stage
    {
        private static readonly Lazy<Canceled> LazyCancel= new (() => new Canceled());
        private static readonly Lazy<Ended> LazyEnd = new (() => new Ended());
        private static readonly Lazy<Errored> LazyError = new (() => new Errored());

        public static Canceled Cancel { get; } = LazyCancel.Value;
        public static Ended End { get; } = LazyEnd.Value;
        public static Errored Error { get; } = LazyError.Value;

        public sealed class Errored : IStage { }
        public sealed class Canceled : IStage { }
        public sealed class Ended : IStage { }
    }

    internal interface IStage
    {
        internal interface IProcess
        {
            UniTask<OneOf<IStage, Stage.Canceled, Stage.Errored>> ProcessAsync(CancellationToken cancellation = default);
        }
    }
}
