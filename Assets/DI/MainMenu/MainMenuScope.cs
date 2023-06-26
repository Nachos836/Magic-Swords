using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    internal sealed class MainMenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<MainMenuEntry>();
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
