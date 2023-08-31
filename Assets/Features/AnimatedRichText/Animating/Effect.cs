using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.AnimatedRichText.Animating
{
    // [Serializable]
    // internal abstract class Effect
    // {
    //     private static readonly Regex CamelCaseToSpaceSeparated = new
    //     (
    //         pattern: @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))",
    //         RegexOptions.Compiled
    //     );
    //
    //     [SerializeField] [UsedImplicitly] [HideInInspector] private string _effectName = CamelCaseToSpaceSeparated
    //         .Replace(input: typeof(Effect).Name, replacement: " $0");
    //
    //     public abstract string Name { get; }
    //     public abstract Tween Tween { get; }
    // }

    internal interface IEffect : ICloneable
    {
        Tween Tween { get; }
    }
}
