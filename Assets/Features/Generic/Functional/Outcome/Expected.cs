using System;

namespace MagicSwords.Features.Generic.Functional.Outcome
{
    public static class Expected
    {
        private static readonly Lazy<Success> LazySuccess = new (static () => new Success());
        private static readonly Lazy<Unit> LazyUnit = new (static () => new Unit());

        private static readonly Lazy<Abortion> LazyAborted = new (static () => new Abortion());
        private static readonly Lazy<Failure> LazyFailed= new (static () => new Failure());
        private static readonly Lazy<Cancellation> LazyCancel = new (static () => new Cancellation());

        public static Success Success { get; } = LazySuccess.Value;
        public static Unit Unit { get; } = LazyUnit.Value;

        public static Abortion Aborted { get; } = LazyAborted.Value;
        public static Failure Failed { get; } = LazyFailed.Value;
        public static Cancellation Canceled { get; } = LazyCancel.Value;

        public sealed class Abortion : IExpected
        {
            string IExpected.Message => "Execution Aborted.";
        }

        public sealed class Failure : IExpected
        {
            string IExpected.Message => "Execution Failed.";
        }

        public sealed class Cancellation : IExpected
        {
            string IExpected.Message => "Execution Cancelled.";
        }
    }

    public interface IExpected
    {
        string Message { get; }
    }
}
