using NUnit.Framework;
using JSONSchema2POCO;
using System;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Shouldly;
using NUnit.Framework.Internal;

namespace JSONSchema2POCO.Tests
{
    public class ClassGeneratorTests
    {
        [Test]
        [TestCase("allOfMultiples")]
        [TestCase("allOfDifferentTypes")]
        [TestCase("allOfNumber")]
        [TestCase("numberType")]
        [TestCase("stringType")]
        [TestCase("stringArray")]
        [TestCase("allOfProperties")]
        [TestCase("allOfTypeString")]
        [TestCase("stringEnum")]
        [TestCase("draft-04")]
        public void Test(string testcase)
        {
            string path = "ClassGeneratorTests/"+ testcase + "/";
            TestSchema(path);
        }
        

        private void TestSchema(string path)
        {
            var schemaJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(path + "input.json")));
            var schema = new JSONSchema(schemaJson);
            var generator = new ClassGeneratorFromJsonSchema(schema);
            generator.GenerateAll();
            var results = generator.PrintAll();

            var expected = File.ReadAllText(Path.GetFullPath(path + "expected.txt"));
            foreach (string value in results.Values)
            {
                Console.WriteLine(value);
            }
            results.Values.Aggregate("", (current, next) => current+next).ShouldBe(expected);
        }
    }
}