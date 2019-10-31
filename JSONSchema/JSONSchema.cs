using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace JSONSchema2POCO
{
    public class JSONSchema
    {
        private static readonly Dictionary<string, List<JsonValueKind>> expectedJsonValueKinds = new Dictionary<string, List<JsonValueKind>>
        {
            {"default", new List<JsonValueKind> {
                JsonValueKind.Object ,
                JsonValueKind.Number,
                JsonValueKind.Undefined,
                JsonValueKind.Array,
                JsonValueKind.False,
                JsonValueKind.Null,
                JsonValueKind.String,
                JsonValueKind.True} },
            {"$ref", new List<JsonValueKind> {JsonValueKind.String } },
            {"id", new List<JsonValueKind> {JsonValueKind.String } },
            {"$schema", new List<JsonValueKind> {JsonValueKind.String } },
            {"title", new List<JsonValueKind> {JsonValueKind.String } },
            {"description", new List<JsonValueKind> {JsonValueKind.String } },
            {"multipleOf", new List<JsonValueKind> {JsonValueKind.Number } },
            {"maximum", new List<JsonValueKind> {JsonValueKind.Number } },
            {"exclusiveMaximum", new List<JsonValueKind> {JsonValueKind.True, JsonValueKind.False } },
            {"minimum", new List<JsonValueKind> {JsonValueKind.Number } },
            {"exclusiveMinimum", new List<JsonValueKind> {JsonValueKind.True, JsonValueKind.False } },
            {"maxLength", new List<JsonValueKind> {JsonValueKind.Number } },
            {"minLength", new List<JsonValueKind> {JsonValueKind.Number } },
            {"pattern", new List<JsonValueKind> {JsonValueKind.String } },
            {"additionalItems", new List<JsonValueKind> {JsonValueKind.False, JsonValueKind.True, JsonValueKind.Object } },
            {"items", new List<JsonValueKind> {JsonValueKind.Object, JsonValueKind.Array } },
            {"maxItems", new List<JsonValueKind> {JsonValueKind.Number } },
            {"minItems", new List<JsonValueKind> {JsonValueKind.Number } },
            {"uniqueItems", new List<JsonValueKind> {JsonValueKind.True, JsonValueKind.False } },
            {"maxProperties", new List<JsonValueKind> {JsonValueKind.Number } },
            {"minProperties", new List<JsonValueKind> {JsonValueKind.Number } },
            {"required", new List<JsonValueKind> {JsonValueKind.Array } },
            {"additionalProperties", new List<JsonValueKind> {JsonValueKind.False, JsonValueKind.True, JsonValueKind.Object } },
            {"definitions", new List<JsonValueKind> {JsonValueKind.Object } },
            {"properties", new List<JsonValueKind> {JsonValueKind.Object } },
            {"patternProperties", new List<JsonValueKind> {JsonValueKind.Object } },
            {"dependencies", new List<JsonValueKind> {JsonValueKind.Object } },
            {"enum", new List<JsonValueKind> {JsonValueKind.Array } },
            {"type", new List<JsonValueKind> {JsonValueKind.String, JsonValueKind.Array } },
            {"format", new List<JsonValueKind> {JsonValueKind.String } },
            {"allOf", new List<JsonValueKind> {JsonValueKind.Array } },
            {"anyOf", new List<JsonValueKind> {JsonValueKind.Array } },
            {"oneOf", new List<JsonValueKind> {JsonValueKind.Array } },
            {"not", new List<JsonValueKind> {JsonValueKind.Object } },
        };


        private static readonly Dictionary<string, Action<JSONSchema, JsonElement>> settersDictionary = new Dictionary<string, Action<JSONSchema, JsonElement>>
        {
            {"id", (x,y) => {x.Id = y.GetString(); } },
            {"$ref", (x,y) => { } },
            {"default", (x,y) => { } },
            {"$schema", (x,y) => {x.Schema = y.GetString(); } },
            {"title", (x,y) => {x.Title = y.GetString(); } },
            {"description", (x,y) => {x.Description = y.GetString(); } },
            {"multipleOf", (x,y) => {x.MultipleOf = y.GetUInt32(); } },
            {"maximum", (x,y) => {x.Maximum = y.GetUInt32(); } },
            {"exclusiveMaximum", (x,y) => {x.ExclusiveMaximum = y.GetBoolean(); } },
            {"minimum", (x,y) => {x.Mininum = y.GetUInt32(); } },
            {"exclusiveMinimum", (x,y) => {x.ExclusiveMinimum = y.GetBoolean(); } },
            {"maxLength", (x,y) => {x.MaxLength = y.GetUInt32(); } },
            {"minLength", (x,y) => {x.MinLength = y.GetUInt32(); } },
            {"pattern", (x,y) => {x.Pattern = y.GetString(); }  },
            {"additionalItems", (x,y) => {
                if(y.ValueKind == JsonValueKind.Object)
                    { x.AdditionalItems = new AnyOf<bool, JSONSchema>(new JSONSchema(y)); }
                else
                {
                    x.AdditionalItems = new AnyOf<bool, JSONSchema>(y.GetBoolean());
                }
                }
            },
            {"items", (x,y) => {
                if(y.ValueKind == JsonValueKind.Object){
                    x.Items = new AnyOf<JSONSchema, SchemaArray>(new JSONSchema(y));
                }
                else
                {
                    x.Items = new AnyOf<JSONSchema, SchemaArray>(new SchemaArray(y));
                }
              }
            },
            {"maxItems", (x,y) => {x.MaxItems = y.GetUInt32(); } },
            {"minItems", (x,y) => {x.MinItems = y.GetUInt32(); } },
            {"uniqueItems", (x,y) => {x.UniqueItems = y.GetBoolean(); } },
            {"maxProperties", (x,y) => {x.MaxProperties = y.GetUInt32(); } },
            {"minProperties", (x,y) => {x.MinProperties = y.GetUInt32(); } },
            {"required", (x,y) => {x.Required = new StringArray(y); }  },
            {"additionalProperties", (x,y) => {
                if(y.ValueKind == JsonValueKind.Object)
                    { x.AdditionalProperties = new AnyOf<bool, JSONSchema>(new JSONSchema(y)); }
                else
                {
                    x.AdditionalProperties = new AnyOf<bool, JSONSchema>(y.GetBoolean());
                }
                }
            },
            {"definitions",  (x,y) => {
                x.Definitions = new Dictionary<string, JSONSchema>();
                foreach(JsonProperty property in y.EnumerateObject())
                {
                    x.Definitions[property.Name]= new JSONSchema(property.Value);
                }
            }
            },
            {"properties",  (x,y) => {
                x.Properties = new Dictionary<string, JSONSchema>();
                foreach(JsonProperty property in y.EnumerateObject())
                {
                    x.Properties[property.Name]= new JSONSchema(property.Value);
                }
            }
            },
            {"patternProperties", (x,y) => {
                x.PatternProperties = new Dictionary<string, JSONSchema>();
                foreach(JsonProperty property in y.EnumerateObject())
                {
                    x.PatternProperties[property.Name]= new JSONSchema(property.Value);
                }
            } 
            },
            {"dependencies", (x,y) => { } },
            {"enum", (x,y) => {
                var list = new List<JsonElement>();
                foreach(JsonElement jsonElement in y.EnumerateArray())
                {
                    list.Add(jsonElement);
                }
                x.Enum = new UniqueNonEmptyList<JsonElement>( list);
            }
            },
            {"type", (x,y) => {
               if(y.ValueKind == JsonValueKind.String)
                {
                    x.Type= new AnyOf<SimpleType,UniqueNonEmptyList<SimpleType>>(new SimpleType(y.GetString()));
                }
                else
                {
                        var list = new List<SimpleType>();
                    foreach(JsonElement jsonElement in y.EnumerateArray())
                    {
                        list.Add(new SimpleType(jsonElement.GetString()));
                    }
                    x.Type= new AnyOf<SimpleType,UniqueNonEmptyList<SimpleType>>(new UniqueNonEmptyList<SimpleType>(list));
                }
            } 
            },
            {"format", (x,y) => {x.Format= y.GetString(); }},
            {"allOf", (x,y) =>{
                x.AllOf = new SchemaArray(y);
            }
            },
            {"anyOf", (x,y) =>{
                x.AnyOf = new SchemaArray(y);
            }
            },
            {"oneOf", (x,y) =>{
                x.OneOf = new SchemaArray(y);
            }
            },
            {"not", (x,y) =>{
                x.Not = new JSONSchema(y);
            }
            }
        };

        public string Id { get; set; }

        public string Schema { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public uint MultipleOf
        {
            get { return MultipleOf; }
            set
            {
                if (value > 0)
                {
                    MultipleOf = value;
                }
                else
                {
                    throw new ArgumentException("MultipleOf must greater than 0");
                }
            }
        }

        public uint Maximum { get; set; }

        public bool ExclusiveMaximum { get; set; }

        public uint Mininum { get; set; }

        public bool ExclusiveMinimum { get; set; }

        public uint MaxLength { get; set; }

        public uint MinLength { get; set; }

        public string Pattern { get; set; }

        public AnyOf<Boolean, JSONSchema> AdditionalItems { get; set; }

        public AnyOf<JSONSchema, SchemaArray> Items { get; set; }

        public uint MaxItems { get; set; }

        public uint MinItems { get; set; }

        public bool UniqueItems { get; set; }

        public uint MaxProperties { get; set; }

        public uint MinProperties { get; set; }

        public StringArray Required { get; set; }

        public AnyOf<Boolean, JSONSchema> AdditionalProperties { get; set; }

        public Dictionary<string, JSONSchema> Definitions { get; set; }

        public Dictionary<string, JSONSchema> Properties { get; set; }

        public Dictionary<string, JSONSchema> PatternProperties { get; set; }

        public UniqueNonEmptyList<JsonElement> Enum { get; set; }

        public AnyOf<SimpleType, UniqueNonEmptyList<SimpleType>> Type { get; set; }

        public string Format { get; set; }

        public SchemaArray AllOf { get; set; }

        public SchemaArray AnyOf { get; set; }

        public SchemaArray OneOf { get; set; }

        public JSONSchema Not { get; set; }

        public JSONSchema(JsonDocument schema) : this(schema.RootElement)
        {
        }

        public JSONSchema(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Invalid Json Schema");
            }

            ObjectEnumerator objectEnumerator = root.EnumerateObject();

            while (objectEnumerator.MoveNext())
            {
                JsonProperty current = objectEnumerator.Current;
                if (expectedJsonValueKinds.ContainsKey(current.Name) && expectedJsonValueKinds[current.Name].Contains(current.Value.ValueKind))
                {
                    settersDictionary[current.Name].Invoke(this, current.Value);
                }
                else
                {
                    throw new ArgumentException("Invalid JSON Schema");
                }
            }

        }



    }

    public class SimpleType
    {
        static readonly HashSet<string> Values = new HashSet<string>(new string[] { "array", "boolean", "integer", "null", "number", "object", "string" });

        public readonly string Value;
        public SimpleType(String value)
        {
            if (Values.Contains(value))
            {
                Value = value;
            }
            else
            {
                throw new ArgumentException(value + " is not a valid SimpleType");
            }
        }

    }



    public class StringArray : UniqueNonEmptyList<string>
    {
        public StringArray(JsonElement jsonElement) : base(transformJsonArrayToList(jsonElement)) { }
  

        private static List<string> transformJsonArrayToList(JsonElement jsonElement)
        {
            List<string> list = new List<string>();
            foreach (JsonElement str in jsonElement.EnumerateArray())
            {
                list.Add(str.GetString());
            }
            return list;
        }
    }

    public class SchemaArray : List<JSONSchema>
    {
        public SchemaArray(JsonElement jsonElement)
        {
            foreach (JsonElement schema in jsonElement.EnumerateArray())
            {
                this.Add(new JSONSchema(schema));
            }
        }
    }
}
