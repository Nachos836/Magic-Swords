using MagicSwords.Features;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.ApplicationEntry
{
    // using Dependencies;
    // using Prerequisites;

    internal sealed class ApplicationEntryScope : LifetimeScope
    {
        // [field: SerializeField]
        // [field: ValidateInput(nameof(DefaultsValidation.ConfigIsProvided))]
        // private Defaults Default { get; set; }

        [Scene, SerializeField] private int _mainMenuScene; 

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.RegisterEntryPoint<EntryPoint>().WithParameter(_mainMenuScene);
            
            // builder.RegisterComponent(_mainMenuScene);
            builder.Register<ISceneSwitcher, SceneSwitcher>(Lifetime.Scoped);

            // builder
            //     .AddApplicationEntry<ApplicationEntryPoint>(Default.GameplayScene)
            //     .AddMessagePipeFeature(out var messagePipeOptions)
            //     .AddPlayerInputFeature(messagePipeOptions)
            //     .AddEnemiesSpawnFeature(Default.EnemiesConfig)
            //     .AddTimeProviderFeature(Default.TimeConfig);
        }
    }
}
