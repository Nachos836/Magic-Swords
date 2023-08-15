using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks.Linq;

namespace MagicSwords.Features.TextAnimator.TextParsing
{
    using Effect;

    internal sealed class TextParser
    {
        private readonly string _rawText;

        public TextParser(string rawText)
        {
            _rawText = rawText;
        }

        public async IAsyncEnumerable<(string, Tween)> ParseAsync
        (
            IEffect[] effects,
            [EnumeratorCancellation] CancellationToken cancellation = default
        ) {
            var document = XDocument.Parse(_rawText, LoadOptions.PreserveWhitespace);

            await foreach (var element in document.Elements().ToUniTaskAsyncEnumerable().TakeUntilCanceled(cancellation))
            {
                var candidates = effects.Where(candidate => candidate.TryMatch(element.Name.LocalName));
                Tween resultedTween = null;

                foreach (var effect in candidates)
                {
                    resultedTween ??= effect.Tween;

                    if (resultedTween is not null) resultedTween += effect.Tween;
                }

                if (resultedTween is not null)
                {
                    yield return (element.Value, resultedTween);
                }
            }
        }
    }
}
