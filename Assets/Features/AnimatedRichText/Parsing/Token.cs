using System;

namespace MagicSwords.Features.AnimatedRichText.Parsing
{
    internal readonly struct Token
    {
        public readonly ReadOnlyMemory<char> Text;
        public readonly ReadOnlyMemory<char>[] Tags;

        public Token(ReadOnlyMemory<char>[] tags, ReadOnlyMemory<char> text)
        {
            Tags = tags;
            Text = text;
        }

        public Token(ReadOnlyMemory<char> text)
        {
            Tags = Array.Empty<ReadOnlyMemory<char>>();
            Text = text;
        }
    }
}
