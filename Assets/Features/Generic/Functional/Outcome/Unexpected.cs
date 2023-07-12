using System;

namespace MagicSwords.Features.Generic.Functional.Outcome
{
    internal static class Unexpected
    {
        private static readonly Lazy<Exception> LazyFailed = new (() => new Exception("Execution is Failed due to Exception"));
        private static readonly Lazy<Exception> LazyError = new (() => new Exception("Execution is Aborted by Exception"));

        public static Exception Failed { get; } = LazyFailed.Value;
        public static Exception Error { get; } = LazyError.Value;
    }
}
