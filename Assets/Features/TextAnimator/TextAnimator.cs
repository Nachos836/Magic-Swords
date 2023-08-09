using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MagicSwords.Features.TextAnimator
{
    public sealed class TextAnimator : MonoBehaviour
    {
        private List<TagSequence> _tags = new();

        // text = "<wobble>Cyka</>"
        public async UniTask PresentAsync(string text,CancellationToken cancellation=default)
        {
            _tags.AddRange(BuildTags(text,cancellation));
            foreach (var tag in _tags)
            {
                await tag.ShowSequence(cancellation);
            }
        }

        // text = "<wobble>Cyka</>"
        private IEnumerable<TagSequence> BuildTags(string text, CancellationToken cancellation)
        {
            int startTag = 0;
            int endTag = 0;
            IEffect effect = default;
            var letters = text[startTag..endTag];
            
            yield return new TagSequence(letters,effect);
            /*
             * Letters = "Cyka"
             * Effect = new Wobble()
             */
        }
    }
}