using NUnit.Framework;
using JSONSchema2POCO;
using System;
using System.Text.Json;
using System.IO;
using System.Diagnostics;

namespace JSONSchema2POCO.Tests
{
    public class ClassGeneratorTests
    {
        private JSONSchema draft04schema;

        [SetUp]
        public void Setup()
        {
            //var draft04SchemaJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath("testSchema.json")));
            var draft04SchemaJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath("draft-04.json")));

            draft04schema = new JSONSchema(draft04SchemaJson);
        }

        [Test]
        public void Test1()
        {
            var generator = new ClassGeneratorFromJsonSchema(draft04schema, "testClass");
            generator.Generate();

            var result = generator.Print();

            Console.WriteLine(result);
            Debug.WriteLine(result);
            Trace.WriteLine(result);
        }
    }
}