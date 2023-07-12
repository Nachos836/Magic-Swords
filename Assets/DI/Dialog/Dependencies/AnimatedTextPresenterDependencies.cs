using System;
using TMPro;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog.Dependencies
{
    using Features.Dialog;
    using Features.Dialog.Stages;
    using Features.Generic.Sequencer;

    internal static class AnimatedTextPresenterDependencies
    {
        public static IContainerBuilder AddAnimatedTextPresenter
        (
            this IContainerBuilder builder,
            AnimatedTextPresenter presenter,
            TextMeshProUGUI field,
            float delay,
            string[] monologue
        ) {
            builder.Register(resolver =>
            {
                var scope = resolver.CreateScope(scopeResolver =>
                {
                    scopeResolver.Register<Setup>(Lifetime.Transient)
                        .WithParameter(field)
                        .WithParameter(delay)
                        .WithParameter(monologue)
                        .AsSelf();

                    scopeResolver.RegisterFactory<string, TextMeshProUGUI, TimeSpan, AutoPrint>(_ =>
                    {
                        return (currentSegment, currentField, currentDelay) =>
                            new AutoPrint(currentSegment, currentField, currentDelay);

                    }, Lifetime.Transient);
                });

                using (scope) return new Sequencer(firstState: scope.Resolve<Setup>());

            }, Lifetime.Transient);

            builder.RegisterComponent(presenter);

            return builder;
        }
    }
}
