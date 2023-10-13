using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace MagicSwords.Features.Text.AnimatedRichText
{
    using Generic.Functional;
    using Animating;
    using Configuring;
    using Configuring.Registry;
    using Parsing;
    using Playing;

    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Create Text")]
    [PreferBinarySerialization]
    public sealed class RichText : ScriptableObject, IText
    {
        [field: Header("Configuration")]
        [field: SerializeField]
        [field: ResizableTextArea]
        [field: OnValueChanged(nameof(ConfigAreNotRelevantAnymore))]
        internal string Text { get; private set; }

        [SerializeField]
        [HideInInspector]
        private IntermediateConfig[] _intermediateConfigs;

        [SerializeField]
        [InspectorName("Config")]
        [HideIf(nameof(ConfigsAreNotExisted))]
        private AnimationConfiguration[] _editableConfig;

        AsyncLazy<AsyncResult> IText.PresentAsync(Player player, CancellationToken cancellation)
        {
            return GenerateResultedConfigAsync(_intermediateConfigs, player, cancellation)
                .ContinueWith(preset => preset.PlayAsync(cancellation))
                .ToAsyncLazy();
        }

#       if UNITY_EDITOR

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

#       endif

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
            return editableConfig.Select(static configuration => new IntermediateConfig
            {
                PlainText = configuration.TextToShow,
                Effects = configuration.EffectConfigs
                    .Select(static effect => effect)
                    .ToArray()

            }).ToArray();
        }

        private static UniTask<Preset> GenerateResultedConfigAsync
        (
            IEnumerable<IntermediateConfig> intermediateConfigs,
            Player player,
            CancellationToken cancellation = default
        ) {
            return intermediateConfigs
                .ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .Select(config => new Preset (
                    config.PlainText,
                    Enumerable.Repeat
                    (
                        config.Effects
                            .Select(static effect => effect.Tween)
                            .DefaultIfEmpty()
                            .Aggregate(static (first, second) => first + second),
                        config.PlainText.Length
                    ).ToArray(),
                    player
                ))
                .AggregateAsync(static (first, second) => first.Append(second), cancellation);
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
        private struct IntermediateConfig
        {
            [SerializeField] public string PlainText;
            [SerializeReference] public IEffect[] Effects;
        }

        internal sealed class Preset
        {
            private readonly string _plainText;
            private readonly Tween[] _tweens;
            private readonly Player _player;

            public Preset(string plainText, Tween[] tweens, Player player)
            {
                _plainText = plainText;
                _tweens = tweens;
                _player = player;
            }

            public Preset Append(Preset another)
            {
                return new Preset
                (
                    _plainText + another._plainText,
                    _tweens.Append(another._tweens),
                    _player
                );
            }

            public async UniTask<AsyncResult> PlayAsync(CancellationToken cancellation = default)
            {
                await _player.PlayAsync(_plainText, _tweens, cancellation);

                return AsyncResult.Success;
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
