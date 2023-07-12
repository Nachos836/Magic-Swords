namespace MagicSwords.Features.Dialog.Payload
{
    using Generic.Functional;

    public readonly struct Message
    {
        private readonly string[] _monologue;
        private readonly int _current;

        public Message(string[] monologue) : this(monologue, default) { }

        private Message(string[] monologue, int current)
        {
            _monologue = monologue;
            _current = current;
        }

        public string Part => _monologue[_current];

        public OptionalResult<Message> Next => _current < _monologue.Length - 1
            ? new Message(_monologue, _current + 1)
            : OptionalResult<Message>.None;
    }
}
