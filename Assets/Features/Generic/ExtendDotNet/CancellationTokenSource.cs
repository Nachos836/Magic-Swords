using System.Threading;

namespace MagicSwords.Features.Generic.ExtendDotNet
{
    public static class CancellationTokenSourceExtensions
    {
        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token)
        {
            return CancellationTokenSource.CreateLinkedTokenSource
            (
                token1: token,
                token2: CancellationToken.None
            );
        }
    }
}
