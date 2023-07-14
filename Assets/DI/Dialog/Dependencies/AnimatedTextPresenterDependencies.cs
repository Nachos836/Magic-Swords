using System;
using TMPro;
using VContainer;
using VContainer.Unity;

namespace MagicSwords.DI.Dialog.Dependencies
{
    using Features.Dialog;
    using Features.Dialog.Stages;
    using Features.Dialog.Payload;
    using Features.Generic.Sequencer;

    internal static class AnimatedTextPresenterDependencies
    {
        public static IContainerBuilder AddAnimatedTextPresenter
        (
            this IContainerBuilder builder,
            TextMeshProUGUI field,
            TimeSpan delay,
            string[] monologue
        ) {
            builder.Register(resolver =>
            {
                var scope = resolver.CreateScope(container =>
                {
                    Func<Message, Print> printing = default;
                    Func<Message, Fetch> fetching = default;

                    container.Register(dependency =>
                    {
                        printing ??= dependency.Resolve<Func<Message, Print>>();

                        return new Initial(printing, monologue);

                    }, Lifetime.Transient);

                    container.Register(dependency =>
                    {
                        fetching ??= dependency.Resolve<Func<Message, Fetch>>();

                        return printing ??= message => new Print(fetching, message, field, delay);

                    }, Lifetime.Transient);

                    container.Register(_ =>
                    {
                        return fetching ??= message => new Fetch(printing, message);

                    }, Lifetime.Transient);
                });

                using (scope) return new Sequencer(firstState: scope.Resolve<Initial>());

            }, Lifetime.Transient);

            builder.RegisterComponentInHierarchy<AnimatedTextPresenter>();

            return builder;
        }
    }
}
