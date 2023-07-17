namespace MagicSwords.Features.Dialog.Payload
{
    using Generic.Functional;

    public interface IFetchMessage
    {
        OptionalResult<Message> Next { get; }
    }

    public readonly struct Message : IFetchMessage
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

        OptionalResult<Message> IFetchMessage.Next => _current < _monologue.Length - 1
            ? new Message(_monologue, _current + 1)
            : OptionalResult<Message>.None;
    }
}
