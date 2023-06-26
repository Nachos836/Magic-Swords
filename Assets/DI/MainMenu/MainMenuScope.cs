using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.MainMenu
{
    internal sealed class MainMenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            UnityEngine.Debug.Log("Вот наше главное меню!");
        }
    }
}
