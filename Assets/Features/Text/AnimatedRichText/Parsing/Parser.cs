using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

using static System.StringComparison;

using RawTag = System.Collections.Generic.KeyValuePair<string, string>;

namespace MagicSwords.Features.Text.AnimatedRichText.Parsing
{
    internal readonly struct Parser : IDisposable
    {
        private const string UniversalMassCloseTag = "</>";

        private readonly ReadOnlyMemory<char> _input;
        private readonly Stack<string> _tokens;
        private readonly Queue<ReadOnlyMemory<char>> _scope;
        private readonly ImmutableDictionary<string, string> _table;

        public Parser(string input, [CanBeNull] IEnumerable<string> tags)
        {
            _input = input.AsMemory();
            _tokens = new Stack<string>(_input.Length / 2);
            _scope = new Queue<ReadOnlyMemory<char>>(_input.Length / 2);
            _table = ImmutableDictionary.CreateRange(tags is not null
                ? tags.Select(static tag => new RawTag($"<{tag}>", $"</{tag}>"))
                : Enumerable.Empty<RawTag>());
        }

        public IEnumerable<Token> Parse()
        {
            var tagMatched = false;
            int blockBegin = default;
            int blockEnd = default;

            for (var current = 0; current < _input.Length;)
            {
                foreach (var openTag in _table.Keys)
                {
                    if (_input.TryMatchTag(current, openTag, out var next))
                    {
                        if (blockEnd != default)
                        {
                            yield return new Token(_input[blockBegin .. blockEnd]);

                            blockEnd = default;
                        }

                        current = next;
                        blockBegin = current;
                        tagMatched = true;
                        _tokens.Push(_table[openTag]);
                        _scope.Enqueue(openTag.GetTagName());
                    }
                }

                {
                    if (_input.TryMatchTag(current, UniversalMassCloseTag, out var next))
                    {
                        if (_tokens.Count is 0) throw new Exception();

                        tagMatched = true;
                        blockEnd = current;

                        var result = blockBegin != blockEnd
                            ? _input[blockBegin .. blockEnd]
                            : ReadOnlyMemory<char>.Empty;

                        current = next;
                        yield return new Token(_scope.ToArray(), result);

                        _tokens.Clear();
                        _scope.Clear();
                        blockBegin = next;
                        blockEnd = default;
                    }
                }

                foreach (var closeTag in _table.Values)
                {
                    if (_input.TryMatchTag(current, closeTag, out var next))
                    {
                        if (_tokens.TryPop(out var matchedClosedTag) is false) throw new Exception();
                        if (matchedClosedTag.Equals(closeTag, InvariantCultureIgnoreCase) is false) throw new Exception();

                        tagMatched = true;
                        if (blockEnd == default) blockEnd = current;

                        if (_tokens.Count is 0)
                        {
                            var result = blockBegin != blockEnd
                                ? _input[blockBegin .. blockEnd]
                                : ReadOnlyMemory<char>.Empty;

                            current = next;
                            yield return new Token(_scope.ToArray(), result);

                            _scope.Clear();
                            blockBegin = next;
                            blockEnd = default;
                        }
                        else
                        {
                            current = next;
                        }
                    }
                }

                if (tagMatched is false)
                {
                    current++;
                    blockEnd = current;
                }
                else
                {
                    tagMatched = false;
                }
            }

            if (_tokens.Count is not 0) throw new Exception();

            if (blockBegin <= blockEnd) yield return new Token(_input[blockBegin .. blockEnd]);
        }

        public void Dispose()
        {
            _tokens.Clear();
            _scope.Clear();
        }
    }

    internal static class TagFiler
    {
        public static ReadOnlyMemory<char> GetTagName(this string tag)
        {
            return tag.AsMemory()[1..^1];
        }
    }

    internal static class TagsComparer
    {
        public static bool TryMatchTag(this ReadOnlyMemory<char> input, int position, string tag, out int nextPosition)
        {
            nextPosition = position + tag.Length;

            if (nextPosition > input.Length) return false;

            return input[position .. nextPosition]
                .Span.Equals(tag, InvariantCultureIgnoreCase);
        }
    }
}
