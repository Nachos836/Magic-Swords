using System;
using Cysharp.Threading.Tasks;
using TMPro;
using VContainer;

namespace MagicSwords.DI.Text.Dependencies
{
    using Features.Text;
    using Features.Input;
    using Features.Generic.Sequencer;
    using Features.Text.AnimatedRichText.Playing.Stages;
    using Features.Text.AnimatedRichText.Playing.Stages.Payload;

    internal static class SequencedTextPresenterDependencies
    {
        public static IContainerBuilder AddSequencedTextPresenter
        (
            this IContainerBuilder builder,
            IText[] monologue
        ) {
            builder.Register(resolver =>
            {
                var scope = resolver.CreateScope(container =>
                {
                    const PlayerLoopTiming yieldPoint = PlayerLoopTiming.Update;

                    IInputFor<ReadingSkip> readingSkipInput = default;
                    Func<Message, Print> printing = default;
                    Func<Message, Fetch> fetching = default;
                    Func<Message, Skip> skipping = default;
                    Func<Message, Delay> delaying = default;

                    TMP_Text field = default;
                    ISymbolsDelay symbolsDelay = default;
                    IMessagesDelay messagesDelay = default;

                    container.Register(dependency =>
                    {
                        printing ??= dependency.Resolve<Func<Message, Print>>();

                        return new Initial(printing, monologue);

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        fetching ??= dependency.Resolve<Func<Message, Fetch>>();
                        skipping ??= dependency.Resolve<Func<Message, Skip>>();
                        readingSkipInput ??= dependency.Resolve<IInputFor<ReadingSkip>>();
                        field ??= dependency.Resolve<TMP_Text>();
                        symbolsDelay ??= dependency.Resolve<ISymbolsDelay>();

                        return printing ??= message => new Print
                        (
                            readingSkipInput,
                            yieldPoint,
                            resolveNext: fetching,
                            resolveSkip: skipping,
                            message,
                            field,
                            symbolsDelay.Value
                        );

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        readingSkipInput ??= dependency.Resolve<IInputFor<ReadingSkip>>();
                        field ??= dependency.Resolve<TMP_Text>();

                        return skipping ??= message => new Skip(readingSkipInput, yieldPoint, InstantPrint, message, field);

                        IStage InstantPrint(Message message) => new Fetch(printing, new Message.Fetcher(message));

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        delaying ??= dependency.Resolve<Func<Message, Delay>>();

                        return fetching ??= message => new Fetch(delaying, new Message.Fetcher(message));

                    }, Lifetime.Scoped);

                    container.Register(dependency =>
                    {
                        messagesDelay ??= dependency.Resolve<IMessagesDelay>();

                        return delaying ??= message => new Delay(yieldPoint, printing, message, messagesDelay.Value);

                    }, Lifetime.Scoped);
                });

                using (scope) return new Sequencer(firstState: scope.Resolve<Initial>());

            }, Lifetime.Scoped);

            return builder;
        }
    }
}
