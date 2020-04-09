﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class OptimizeFileNameFixture : BaseFixture
    {
        public class ExecuteTests : OptimizeFileNameFixture
        {
            [TestCase(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:?#[]@!$&'()*+,;=",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789")]
            [TestCase("Děku.jemeविकीвики-движка", "děku.jemeविकीвикидвижка")]
            [TestCase(
                "this is my title - and some \t\t\t\t\n   clever; (piece) of text here: [ok].",
                "this-is-my-title-and-some-clever-piece-of-text-here-ok")]
            [TestCase(
                "this is my title?!! science and #firstworldproblems :* :sadface=true",
                "this-is-my-title-science-and-firstworldproblems-sadfacetrue")]
            [TestCase(
                "one-two-three--four--five and a six--seven--eight-nine------ten",
                "onetwothreefourfive-and-a-sixseveneightnineten")]
            public async Task FileNameIsConvertedCorrectly(string input, string output)
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath(input));
                OptimizeFileName fileName = new OptimizeFileName();

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            [Test]
            public async Task FileNameShouldBeLowercase()
            {
                // Given
                const string input = "FileName With MiXeD CapS";
                const string output = "filename-with-mixed-caps";
                TestDocument document = new TestDocument(new NormalizedPath(input));
                OptimizeFileName fileName = new OptimizeFileName();

                // When
                TestDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(output);
            }

            public static readonly char[] ReservedChars = NormalizedPath.OptimizeFileNameReservedChars.Where(x => x != '\\' && x != '/').ToArray();

            [TestCaseSource(nameof(ReservedChars))]
            public async Task FileNameIsConvertedCorrectlyWithReservedChar(char character)
            {
                // Given
                string manyCharactersWow = new string(character, 10);
                string path = string.Format("/a/b/c/testing {0} some of {0} these {0}.foo", manyCharactersWow);
                TestDocument document = new TestDocument()
                {
                    new MetadataItem("MyKey", path)
                };
                OptimizeFileName fileName = new OptimizeFileName("MyKey");

                // When
                IDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.GetString("MyKey").ShouldBe("/a/b/c/testing-some-of-these-.foo");
            }

            [TestCase(null)]
            [TestCase("")]
            public async Task IgnoresNullOrEmptyStrings(string input)
            {
                // Given
                TestDocument document = new TestDocument
                {
                    new MetadataItem("MyKey", input)
                };
                OptimizeFileName fileName = new OptimizeFileName("MyKey");

                // When
                IDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.GetString("MyKey").ShouldBe(input);
            }

            [Test]
            public async Task PreservesExtension()
            {
                // Given
                const string input = "myfile.html";
                const string output = "myfile.html";

                TestDocument document = new TestDocument()
                {
                    new MetadataItem("MyKey", input)
                };
                OptimizeFileName fileName = new OptimizeFileName("MyKey");

                // When
                IDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.GetPath("MyKey").FullPath.ShouldBe(output);
            }

            [Test]
            public async Task TrimWhitespace()
            {
                // Given
                const string input = "   myfile.html   ";
                const string output = "myfile.html";
                TestDocument document = new TestDocument
                {
                    new MetadataItem("MyKey", input)
                };
                OptimizeFileName fileName = new OptimizeFileName("MyKey");

                // When
                IDocument result = await ExecuteAsync(document, fileName).SingleAsync();

                // Then
                result.GetPath("MyKey").FullPath.ShouldBe(output);
            }
        }
    }
}
