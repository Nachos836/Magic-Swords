using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace MagicSwords.Features.AnimatedRichText
{
    using Animating;
    using Configuring;
    using Configuring.Registry;
    using Parsing;

    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Create Text")]
    // [PreferBinarySerialization]
    internal sealed class RichText : ScriptableObject
    {
        [field: Header("Configuration")]
        [field: SerializeField]
        [field: ResizableTextArea]
        [field: OnValueChanged(nameof(ConfigAreNotRelevantAnymore))]
        public string Text { get; private set; }

        [SerializeField]
        [HideInInspector]
        private IntermediateConfig[] _intermediateConfigs;

        [SerializeField]
        [InspectorName("Config")]
        [HideIf(nameof(ConfigsAreNotExisted))]
        private AnimationConfiguration[] _editableConfig;

        [CanBeNull] private Config _generatedConfigCached;
        public Config GeneratedConfig => _generatedConfigCached ??= GenerateResultedConfig(_intermediateConfigs);

        [Button] [UsedImplicitly]
        private void Configure()
        {
            _editableConfig = GenerateEditableConfig(Text);

            Save();

            Debug.LogWarning($@"Asset: ""{name}"" was configured!", this);
        }

        [Button] [UsedImplicitly]
        private void Save()
        {
            _intermediateConfigs = GenerateIntermediateConfigs(_editableConfig);

            _configAreRelevant = true;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static AnimationConfiguration[] GenerateEditableConfig(string richText)
        {
            var configsRegistry = Resources
                .FindObjectsOfTypeAll<EffectConfigsRegistry>()
                .Single();

            using var parser = new Parser(richText, configsRegistry.EffectsTags);
            var blocks = parser.Parse();
            var animatorConfigurator = new EffectsConfigurator(blocks, configsRegistry);
            return animatorConfigurator.PopulateConfigurations();
        }

        private static IntermediateConfig[] GenerateIntermediateConfigs(IEnumerable<AnimationConfiguration> editableConfig)
        {
            return editableConfig.Select(configuration => new IntermediateConfig
            {
                PlainText = configuration.TextToShow,
                Effects = configuration.EffectConfigs
                    .Select(effect => effect)
                    .ToArray()

            }).ToArray();
        }

        private static Config GenerateResultedConfig(IEnumerable<IntermediateConfig> intermediateConfigs)
        {
            var generatedConfig = intermediateConfigs.Select(config => new Config (
                config.PlainText,
                Enumerable.Repeat
                (
                    config.Effects
                        .Select(effect => effect.Tween)
                        .DefaultIfEmpty()
                        .Aggregate((first, second) => first + second),
                    config.PlainText.Length
                ).ToArray()
            ))
            .Aggregate((first, second) => new Config (
                first.PlainText + second.PlainText,
                first.Tweens.Append(second.Tweens)
            ));

            return generatedConfig;
        }

#       region UI Validation

        [Header("Validation")]
        [SerializeField]
        [ReadOnly]
        [ValidateInput(nameof(ConfigAreRelevant), @"Make sure you perform ""Configure"" at the end of editing")]
        [Label("Generated config status")]
        [HideIf(nameof(ConfigAreRelevant))]
        private bool _configAreRelevant = true;

        private bool ConfigsAreNotExisted() => _editableConfig is null or { Length:0 };
        private void ConfigAreNotRelevantAnymore() => _configAreRelevant = false;
        private bool ConfigAreRelevant() => _configAreRelevant;

#       endregion
        
        [Serializable]
        private sealed class IntermediateConfig
        {
            [SerializeField] public string PlainText;
            [SerializeReference] public IEffect[] Effects;
        }

        internal sealed class Config
        {
            public readonly string PlainText;
            public readonly Tween[] Tweens;

            public Config(string plainText, Tween[] tweens)
            {
                PlainText = plainText;
                Tweens = tweens;
            }
        }
    }

    internal static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] arrayInitial, T[] arrayToAppend)
        {
            var ret = new T[arrayInitial.Length + arrayToAppend.Length];
            arrayInitial.CopyTo(ret, 0);
            arrayToAppend.CopyTo(ret, arrayInitial.Length);

            return ret;
        }
    }
}
