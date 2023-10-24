using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Animating.Trigger
{
    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Effects/Create Trigger Effect Config")]
    internal sealed class TriggerEffectConfig : EffectConfig
    {
        protected override (string Name, IEffect Effect) ProvideConfiguration() => (Name: "wobble", new TriggerEffect());
    }
}
