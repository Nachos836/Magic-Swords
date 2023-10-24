using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText.Configuring
{
    using Animating;
    using Registry;
    using Parsing;

    [Serializable]
    internal sealed class EffectsConfigurator
    {
        private readonly IEnumerable<Token> _blocks;
        private readonly EffectConfigsRegistry _effectConfigsRegistry;

        [SerializeField] private AnimationConfiguration[]? _configurations;

        public EffectsConfigurator(IEnumerable<Token> blocks, EffectConfigsRegistry effectConfigsRegistry)
        {
            _blocks = blocks;
            _effectConfigsRegistry = effectConfigsRegistry;
            _configurations = null;
        }

        public AnimationConfiguration[] PopulateConfigurations()
        {
            return _configurations ??= _blocks.Select(block => new AnimationConfiguration
            (
                block.Text.ToString(),
                block.Tags.Select(scope =>
                {
                    return _effectConfigsRegistry.PickEffect(scope).Match
                    (
                        some: static effect => effect,
                        none: static () => throw new Exception()
                    );

                }).ToArray()

            )).ToArray();
        }
    }

    [Serializable]
    internal sealed class AnimationConfiguration
    {
        [UsedImplicitly] [SerializeField] [HideInInspector] private string _nameOfConfiguration;
        [field: SerializeField] [field: HideInInspector] public string TextToShow { get; private set; }
        [field: SerializeReference] public IEffect[] EffectConfigs { get; private set; }

        public AnimationConfiguration(string textToShow, IEffect[] effectConfigs)
        {
            TextToShow = textToShow;
            EffectConfigs = effectConfigs;

            _nameOfConfiguration = $@"Animated Text: ""{textToShow}""";
        }
    }
}
