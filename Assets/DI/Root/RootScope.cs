using NaughtyAttributes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Root
{
    using Dependencies;
    using Prerequisites;

    internal sealed class RootScope : LifetimeScope
    {
        [field: Header("Configuration")]

        [field: SerializeField]
        [field: ValidateInput(nameof(DefaultsValidation.ConfigIsProvided))]
        [field: Label("Default Settings")]
        private Defaults Default { get; set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddApplicationEntry()
                .AddMessagePipeFeature(out var messagePipeOptions)
                .AddSceneLoaderFeature(Default.MainMenuSceneReference)
                .AddLogger();
        }
    }
}
