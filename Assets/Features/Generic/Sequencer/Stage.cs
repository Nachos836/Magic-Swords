using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Generic.Sequencer
{
    using Functional;

    public static class Stage
    {
        private static readonly Lazy<Ended> LazyEnd = new (static () => new Ended());

        public static Ended End { get; } = LazyEnd.Value;

        public sealed class Ended : IStage { }
    }

    public interface IStage
    {
        public interface IProcess
        {
            UniTask<AsyncResult<IStage>> ProcessAsync(CancellationToken cancellation = default);
        }
    }
}
