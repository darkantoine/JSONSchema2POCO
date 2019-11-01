using NUnit.Framework;
using JSONSchema2POCO;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace JSONSchema2POCO.Tests
{
    public class JSONSchemaTests
    {
        JsonDocument stringTypeJson;
        JsonDocument simpleSchemaJson;
        JsonDocument draft04SchemaJson;

        [SetUp]
        public void Setup()
        {
            stringTypeJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath("stringType.json")));
            simpleSchemaJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath("simpleSchema.json")));
            draft04SchemaJson = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath("draft-04.json")));
        }

        [Test]
        public void TestStringTypeSchema()
        {            
            JSONSchema stringTypeSchema = new JSONSchema(stringTypeJson);
            TestSimpleType(stringTypeSchema, "string");
        }

        private void TestSimpleType(JSONSchema stringTypeSchema, string typeName)
        {
            Assert.AreEqual(typeof(SimpleType), stringTypeSchema.Type.GetUnderlyingType());
            Assert.That(stringTypeSchema.Type.GetValue() is SimpleType);
            SimpleType simpleType = stringTypeSchema.Type.GetValue() as SimpleType;
            Assert.AreEqual(typeName, simpleType.Value);
        }

        [Test]
        public void TestSimpleSchema()
        {
            JSONSchema simpleSchema = new JSONSchema(simpleSchemaJson);
            TestSimpleType(simpleSchema, "object");

            Assert.AreEqual("simpleSchema", simpleSchema.Title);

            Assert.AreEqual(typeof(JSONSchema), simpleSchema.AdditionalProperties.GetUnderlyingType());
            JSONSchema additionalPropertiesSchema = simpleSchema.AdditionalProperties.GetValue() as JSONSchema;
            TestSimpleType(additionalPropertiesSchema, "string");

            Assert.AreEqual("id", simpleSchema.Required[0]);
            Assert.AreEqual(2, simpleSchema.Properties.Count);
            Assert.That(simpleSchema.Properties.ContainsKey("id"));
            Assert.That(simpleSchema.Properties.ContainsKey("field1"));

            JSONSchema idSchema = simpleSchema.Properties["id"];
            TestSimpleType(idSchema, "integer");

            JSONSchema field1Schema = simpleSchema.Properties["field1"];
            TestSimpleType(field1Schema, "array");

            JSONSchema itemsSchema = field1Schema.Items.GetValue() as JSONSchema;
            TestSimpleType(itemsSchema, "string");
        }

        [Test]
        public void TestDraft04Schema()
        {
            JSONSchema draft04Schema = new JSONSchema(draft04SchemaJson);

            Console.WriteLine(draft04Schema.ToString());

            TestSimpleType(draft04Schema, "object");

 

            Assert.That(draft04Schema.Properties.ContainsKey("id"));
            Assert.That(draft04Schema.Properties.ContainsKey("$schema"));

            JSONSchema idSchema = draft04Schema.Properties["id"];
            TestSimpleType(idSchema, "string");

            JSONSchema additionalItems = draft04Schema.Properties["additionalItems"];
            Assert.NotNull(additionalItems.AnyOf);

            SchemaArray anyOf = additionalItems.AnyOf as SchemaArray;
            TestSimpleType(anyOf[0], "boolean");
            Assert.AreEqual(draft04Schema, draft04Schema.Properties["not"]);
        }
    }
}