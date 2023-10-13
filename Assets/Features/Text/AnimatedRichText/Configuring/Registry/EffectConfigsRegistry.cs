using System;
using System.Collections.Generic;
using System.Linq;
using MagicSwords.Features.Text.AnimatedRichText.Animating;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Configuring.Registry
{
    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Create Effect Configs Repository")]
    internal sealed class EffectConfigsRegistry : ScriptableObject
    {
        [SerializeReference] private EffectConfig[] _effectConfigs;

        public IEnumerable<string> EffectsTags => _effectConfigs.Select(config => config.Name);

        public bool TryPickEffect(ReadOnlyMemory<char> effectName, out IEffect effect)
        {
            effect = _effectConfigs
                .SingleOrDefault(config => config.Name.TryMatchAsRawStrings(effectName))?.Effect
                .Clone() as IEffect;

            return effect is not null;
        }
    }

    internal static class RawStringComparator
    {
        public static bool TryMatchAsRawStrings(this string first, ReadOnlyMemory<char> second)
        {
            var stringsAreEqual = first.Length == second.Length;

            if (stringsAreEqual is false) return false;
            var firstRaw = first.AsSpan();
            for (var i = 0; i < firstRaw.Length; i++)
            {
                stringsAreEqual = firstRaw[i] == second.Span[i];

                if (stringsAreEqual is false) return false;
            }

            return true;
        }
    }
}
