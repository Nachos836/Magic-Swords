using NaughtyAttributes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.ApplicationEntry
{
    using Dependencies;
    using Prerequisites;

    internal sealed class ApplicationEntryScope : LifetimeScope
    {
        [field: SerializeField]
        [field: ValidateInput(nameof(DefaultsValidation.ConfigIsProvided))]
        private Defaults Default { get; set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder
                .AddApplicationEntry<EntryPoint>(Default.MainMenuScene)
                .AddMessagePipeFeature(out var messagePipeOptions)
                .AddSceneLoaderFeature();
        }
    }
}
