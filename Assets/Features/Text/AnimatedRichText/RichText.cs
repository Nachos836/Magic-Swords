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
    using Animating;
    using Configuring;
    using Configuring.Registry;
    using Parsing;

    internal interface ITextWithPrewarm
    {
        IText PrewarmLazyAsync(CancellationToken cancellation = default);
    }

    [CreateAssetMenu(menuName = "Novel Framework/Rich Text/Create Text")]
    [PreferBinarySerialization]
    internal sealed class RichText : ScriptableObject, IText, ITextWithPrewarm
    {
        [Header("Configuration")]
        [SerializeField]
        [ResizableTextArea]
        [OnValueChanged(nameof(ConfigAreNotRelevantAnymore))]
        private string _text = string.Empty;

        [SerializeField]
        [HideInInspector]
        private IntermediateConfig[] _intermediateConfigs = Array.Empty<IntermediateConfig>();

        [SerializeField]
        [InspectorName("Config")]
        [HideIf(nameof(ConfigsAreNotExisted))]
        private AnimationConfiguration[] _editableConfig = Array.Empty<AnimationConfiguration>();

        private AsyncLazy<Preset>? _lazyPreset;

        private AsyncLazy<Preset> LazyPreset => _lazyPreset ??= GenerateResultedConfigAsync(_intermediateConfigs, Application.exitCancellationToken);

        IText ITextWithPrewarm.PrewarmLazyAsync(CancellationToken cancellation)
        {
            _lazyPreset = GenerateResultedConfigAsync(_intermediateConfigs, Application.exitCancellationToken);

            return this;
        }

        AsyncLazy<Preset> IText.ProvidePresetAsync(CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested)
            {
                LazyPreset.Task.AttachExternalCancellation(cancellation)
                    .SuppressCancellationThrow();

                return UniTask.Never<Preset>(cancellation)
                    .ToAsyncLazy();
            }

            return LazyPreset;
        }

#       if UNITY_EDITOR

        [Button] [UsedImplicitly]
        private void Configure()
        {
            _editableConfig = GenerateEditableConfig(_text);

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

        private static AsyncLazy<Preset> GenerateResultedConfigAsync
        (
            IEnumerable<IntermediateConfig> intermediateConfigs,
            CancellationToken cancellation = default
        ) {
            return intermediateConfigs
                .ToUniTaskAsyncEnumerable()
                .TakeUntilCanceled(cancellation)
                .Select(static config => new Preset
                (
                    config.PlainText,
                    Enumerable.Repeat
                    (
                        config.Effects
                            .Select(static effect => effect.Tween)
                            .DefaultIfEmpty()
                            .Aggregate(static (first, second) => first + second),
                        config.PlainText.Length
                    ).ToArray()
                ))
                .AggregateAsync(static (first, second) => first.Append(second), cancellation)
                .ToAsyncLazy();
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

        internal sealed record Preset(string PlainText, Tween[] Tweens)
        {
            public Preset Append(Preset another) => new
            (
                PlainText + another.PlainText,
                Tweens.Append(another.Tweens)
            );
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
