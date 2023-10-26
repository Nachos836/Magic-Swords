namespace MagicSwords.Features.Text.AnimatedRichText.Playing.Stages.Payload
{
    using Generic.Functional;

    internal readonly struct Message
    {
        private readonly IText[] _monologue;
        private readonly int _current;

        public Message(IText[] monologue) : this(monologue, current: default) { }

        private Message(IText[] monologue, int current)
        {
            _monologue = monologue;
            _current = current;
        }

        public IText Part => _monologue[_current];

        private Optional<Message> Next => _current < _monologue.Length - 1
            ? Optional<Message>.Some(new Message(_monologue, _current + 1))
            : Optional<Message>.None;

        internal readonly struct Fetcher
        {
            private readonly Message _message;

            public Fetcher(Message message) => _message = message;

            public Optional<Message> Next => _message.Next;
        }
    }
}
