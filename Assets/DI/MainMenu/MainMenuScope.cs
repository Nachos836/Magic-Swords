using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    using Features.MainMenu;

    internal sealed class MainMenuScope : LifetimeScope
    {
        [field: SerializeField] public MainMenuViewModel MainMenuViewModel { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<MainMenuEntry>();
            builder.Register<MainMenuModel>(Lifetime.Scoped);
            builder.RegisterComponent(MainMenuViewModel);
        }
    }

    internal sealed class MainMenuEntry : IAsyncStartable
    {
        UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Вот наше главное меню!");

            return UniTask.CompletedTask;
        }
    }
}
