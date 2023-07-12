using System;

namespace MagicSwords.Features.Generic.Functional.Outcome
{
    internal static class Expected
    {
        private static readonly Lazy<Success> LazySuccess = new (() => new Success());

        private static readonly Lazy<Abortion> LazyAborted = new (() => new Abortion());
        private static readonly Lazy<Failure> LazyFailed= new (() => new Failure());
        private static readonly Lazy<Cancellation> LazyCancel = new (() => new Cancellation());

        public static Success Success { get; } = LazySuccess.Value;

        public static Abortion Aborted { get; } = LazyAborted.Value;
        public static Failure Failed { get; } = LazyFailed.Value;
        public static Cancellation Canceled { get; } = LazyCancel.Value;

        internal sealed class Abortion : IExpected
        {
            string IExpected.Message => "Execution Aborted.";
        }

        internal sealed class Failure : IExpected
        {
            string IExpected.Message => "Execution Failed.";
        }

        internal sealed class Cancellation : IExpected
        {
            string IExpected.Message => "Execution Cancelled.";
        }
    }

    internal interface IExpected
    {
        string Message { get; }
    }
}
