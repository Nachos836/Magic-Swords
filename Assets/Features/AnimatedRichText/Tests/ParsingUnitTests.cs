using System;
using System.Linq;
using NUnit.Framework;

namespace MagicSwords.Features.AnimatedRichText.Tests
{
    using Parsing;

    [TestFixture]
    internal sealed class ParsingUnitTests
    {
        private static string[] ParsingBase(string input, string[] tags)
        {
            TestContext.Out.WriteLine
            (
                @" --- Parsing ""{0}""
 --- with tags: {1}",
                input,
                tags.DefaultIfEmpty("none").Aggregate((first, second) => $@"""{first}"" ""{second}"" ")
            );

            using var parser = new Parser(input, tags);

            var result = parser.Parse()
                .Select(block => new []
                {
                    block.Tags.DefaultIfEmpty().Aggregate((first, second) =>
                    {
                        var chars = new char[first.Length + second.Length];
                        var destination = new Memory<char>(chars);

                        first.TryCopyTo(destination);
                        second.TryCopyTo(destination[first.Length..]);

                        return new ReadOnlyMemory<char>(chars);

                    }).ToString(),

                    block.Text.ToString()
                })
                .SelectMany(strings => strings)
                .ToArray();

            TestContext.Out.WriteLine(" --- Result is: ");
            foreach (var (part, index) in result.Select((part, index) => (part, index)))
            {
                if (index % 2 is 0)
                {
                    TestContext.Out.Write(@" --- - scope: ""{0}"" ", part);
                }
                else
                {
                    TestContext.Out.WriteLine(@"value: ""{0}""", part);
                }
            }

            TestContext.Out.WriteLine(" --- --- --- ");

            return result;
        }

        [Test]
        public void TryParseEmptyInputEmptyTags()
        {
            const string input = "";
            string[] tags = {};
            
            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "", ""
            }));
        }

        [Test]
        public void TryParseDummyInputEmptyTags()
        {
            const string input = "Hello World!";
            string[] tags = {};

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "", "Hello World!"
            }));
        }

        [Test]
        public void TryParseDummyInputRegularTags()
        {
            const string input = "Hello World!";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "", "Hello World!"
            }));
        }

        [Test]
        public void TryParseRegularInputUnmatchedTag()
        {
            const string input = "<wobble>Hello World!</wobble>";
            string[] tags = { "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "", "<wobble>Hello World!</wobble>"
            }));
        }

        [Test]
        public void TryParseRegularInputRegularTags()
        {
            const string input = "<wobble>Hello World!</wobble>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello World!"
            }));
        }

        [Test]
        public void TryParseRegularInputVariadicCaseTags()
        {
            const string input = "<wObBle>Hello World!</WObble>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello World!"
            }));
        }

        [Test]
        public void TryParseRegularInputUnclosedTag_ShouldThrowException()
        {
            const string input = "Hello World!</wobble>";
            string[] tags = { "wobble", "trigger" };
            string[] result = null;

            Assert.Throws<Exception>(() =>
            {
                result = ParsingBase(input, tags);
            });
            Assert.That(result, Is.Not.EqualTo(new[]
            {
                "wobble", "Hello World!"
            }));
        }

        [Test]
        public void TryParseRegularInputUnclosedUniversalTag_ShouldThrowException()
        {
            const string input = "Hello World!</>";
            string[] tags = { "wobble", "trigger" };
            string[] result = null;

            Assert.Throws<Exception>(() =>
            {
                result = ParsingBase(input, tags);
            });
            Assert.That(result, Is.Not.EqualTo(new[]
            {
                "", "Hello World!"
            }));
        }
        
        [Test]
        public void TryParseRegularInputRegularTags_TwoMatchedTags()
        {
            const string input = "<wobble>Hello</wobble><trigger>World!</trigger>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello",
                "trigger", "World!"
            }));
        }

        [Test]
        public void TryParseRegularInputRegularTags_DoubleMatchedTags()
        {
            const string input = "<wobble><trigger>Hello World!</trigger></wobble>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobbletrigger", "Hello World!"
            }));
        }

        [Test]
        public void TryParseOneTaggedInputRegularTags_TwoMatchedTags()
        {
            const string input = "Hello <trigger>World!</trigger>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "", "Hello ",
                "trigger", "World!"
            }));
        }

        [Test]
        public void TryParseTwoTaggedInputRegularTags_TwoMatchedTags()
        {
            const string input = "<wobble>Hello</wobble> Cyka <trigger>World!</trigger>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello",
                "", " Cyka ",
                "trigger", "World!"
            }));
        }

        [Test]
        public void TryParseOneTaggedInputRegularTags_OneMatchedTag()
        {
            const string input = "<wobble>Hello</wobble> Cyka World!";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello",
                "", " Cyka World!"
            }));
        }

        [Test]
        public void TryParseRegularInputRegularTags_UniversalClose()
        {
            const string input = "<wobble>Hello World!</>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello World!"
            }));
        }

        [Test]
        public void TryParseRegularInputRegularTags_TwoMatchedTags_UniversalClose()
        {
            const string input = "<wobble>Hello</><trigger>World!</>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello",
                "trigger", "World!"
            }));
        }

        [Test]
        public void TryParseRegularInputWithUntaggedRegularTags_TwoMatchedTags_UniversalClose()
        {
            const string input = "<wobble>Hello</> Cyka <trigger>World!</>";
            string[] tags = { "wobble", "trigger" };

            Assert.That(ParsingBase(input, tags), Is.EqualTo(new[]
            {
                "wobble", "Hello",
                "", " Cyka ",
                "trigger", "World!"
            }));
        }
    }
}
