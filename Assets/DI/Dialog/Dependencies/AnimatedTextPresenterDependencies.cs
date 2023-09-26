using System;
using Cysharp.Threading.Tasks;
using TMPro;
using VContainer;

namespace MagicSwords.DI.Dialog.Dependencies
{
    using Features.Input;
    using Features.Dialog.Stages;
    using Features.Dialog.Stages.Payload;
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

                    IInputFor<SubmitAction> submitAction = default;
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
                        submitAction ??= dependency.Resolve<IInputFor<SubmitAction>>();

                        return printing ??= message => new Print
                        (
                            submitAction,
                            yieldPoint,
                            resolveNext: fetching,
                            resolveSkip: skipping,
                            message,
                            field,
                            symbolsDelay
                        );

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        submitAction ??= dependency.Resolve<IInputFor<SubmitAction>>();

                        return skipping ??= message => new Skip(submitAction, yieldPoint, InstantPrint, message, field);

                        IStage InstantPrint(Message message) => new Fetch(printing, new Message.Fetcher(message));

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        delaying ??= dependency.Resolve<Func<Message, Delay>>();

                        return fetching ??= message => new Fetch(delaying, new Message.Fetcher(message));

                    }, Lifetime.Scoped);

                    container.Register(_ =>
                    {
                        return delaying ??= message => new Delay(yieldPoint, printing, message, messagesDelay);

                    }, Lifetime.Scoped);
                });

                using (scope) return new Sequencer(firstState: scope.Resolve<Initial>());

            }, Lifetime.Scoped);

            return builder;
        }
    }
}
