using UnityEngine;

namespace MagicSwords.Features.AnimatedRichText.Animating.Wobble
{
    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Effects/Create Wobble Effect Config")]
    internal sealed class WobbleEffectConfig : EffectConfig
    {
        protected override (string Name, IEffect Effect) ProvideConfiguration()
        {
            return (Name: "wobble", new WobbleEffect());
        }
    }
}
