using NaughtyAttributes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root
{
    using Common;
    using Dependencies;
    using Prerequisites;

    internal sealed class RootScope : LifetimeScope
    {
        [Header("Configuration")]
        [SerializeField]
        [ValidateInput(nameof(DefaultsValidation.ConfigIsProvided))]
        [Label("Default Settings")]
        private Defaults _default;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddUnityBasedLogger(out var logger)
                .AddApplicationEntry(logger)
                .AddMessagePipeFeature()
                .AddSceneLoaderFeature(_default.MainMenuSceneReference, loadInstantly: true);
        }
    }
}
