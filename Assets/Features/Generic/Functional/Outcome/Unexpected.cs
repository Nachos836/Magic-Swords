using System;
using System.Diagnostics;

namespace MagicSwords.Features.Generic.Functional.Outcome
{
    internal static class Unexpected
    {
        private static readonly Lazy<Exception> LazyError = new (static () => new Exception("Execution is Aborted by Exception"));
        private static readonly Lazy<UnreachableException> LazyUnreachable = new (static () => new UnreachableException("Execution shouldn't have happened"));

        public static Exception Error { get; } = LazyError.Value;
        public static UnreachableException Impossible { get; } = LazyUnreachable.Value;
    }
}
