using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.AnimatedRichText.Animating
{
    // [Serializable]
    // internal abstract class EffectBuilder
    // {
    //     private static readonly Regex CamelCaseToSpaceSeparated = new
    //     (
    //         pattern: @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))",
    //         RegexOptions.Compiled
    //     );
    //
    //     [SerializeField] [UsedImplicitly] [HideInInspector] private string _effectName;
    //
    //     protected EffectBuilder(string name)
    //     {
    //         _effectName = CamelCaseToSpaceSeparated.Replace(input: name, replacement: " $0");
    //     }
    //
    //     public abstract Effect BuildEffect();
    // }
}
