using System;
using Cysharp.Threading.Tasks;
using MagicSwords.Features.Dialog.Stages.Payload;
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
            TextMeshProUGUI field,
            TimeSpan symbolsDelay,
            TimeSpan messagesDelay,
            string[] monologue
        ) {
            builder.Register(resolver =>
            {
                var scope = resolver.CreateScope(container =>
                {
                    const PlayerLoopTiming yieldPoint = PlayerLoopTiming.Update;

                    Func<Message, Print> printing = default;
                    Func<Message, Fetch> fetching = default;
                    Func<Message, Skip> skipping = default;
                    Func<Message, Delay> delaying = default;

                    container.Register(dependency =>
                    {
                        printing ??= dependency.Resolve<Func<Message, Print>>();

                        return new Initial(printing, monologue);

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        fetching ??= dependency.Resolve<Func<Message, Fetch>>();
                        skipping ??= dependency.Resolve<Func<Message, Skip>>();

                        return printing ??= message => new Print
                        (
                            yieldPoint, 
                            fetching,
                            skipping,
                            message,
                            field,
                            symbolsDelay
                        );

                    }, Lifetime.Scoped);

                    container.Register(_ =>
                    {
                        IStage InstantPrint(Message message) => new Fetch(printing, message);

                        return skipping ??= message => new Skip(yieldPoint, InstantPrint, message, field);

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        delaying ??= dependency.Resolve<Func<Message, Delay>>();

                        return fetching ??= message => new Fetch(delaying, message);

                    }, Lifetime.Scoped);

                    container.Register(_ =>
                    {
                        return delaying ??= message => new Delay(yieldPoint, printing, message, messagesDelay);

                    }, Lifetime.Scoped);
                });

                using (scope) return new Sequencer(firstState: scope.Resolve<Initial>());

            }, Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<AnimatedTextPresenter>();

            return builder;
        }
    }
}
