using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text
{
    using AnimatedRichText;

    internal interface IText
    {
        AsyncLazy<RichText.Preset> ProvidePresetAsync(CancellationToken cancellation = default);
    }
}
