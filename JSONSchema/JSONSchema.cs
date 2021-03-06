﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.Json;
using static System.Text.Json.JsonElement;
using System.Text;
using System.Reflection;
using System.Linq;
using static JSONSchema2POCO.Utils;
using System.Diagnostics;

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
            {"$ref", (x,y) => {x.Ref = y.GetString(); } },
            {"default", (x,y) => { x.Default = y; } },
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
                    x.Definitions[property.Name].Title = x.Definitions[property.Name].Title ?? property.Name;
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

        public string Ref { get; set; }

        public string Description { get; set; }

        public UInt32? MultipleOf
        {
            get; set;
        }

        public UInt32? Maximum { get; set; }

        public bool? ExclusiveMaximum { get; set; }

        public UInt32? Mininum { get; set; }

        public bool? ExclusiveMinimum { get; set; }

        public UInt32? MaxLength { get; set; }

        public UInt32? MinLength { get; set; }

        public string Pattern { get; set; }

        public AnyOf<Boolean, JSONSchema> AdditionalItems { get; set; }

        public AnyOf<JSONSchema, SchemaArray> Items { get; set; }

        public UInt32? MaxItems { get; set; }

        public UInt32? MinItems { get; set; }

        public bool? UniqueItems { get; set; }

        public UInt32? MaxProperties { get; set; }

        public UInt32? MinProperties { get; set; }

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
        public JsonElement? Default { get; private set; }

        public override string ToString()
        {
            return ToString(0, new HashSet<JSONSchema>());
        }

        private string ToString(int indent, HashSet<JSONSchema> visited)
        {
            StringBuilder sb = new StringBuilder();
            if (visited.Contains(this)){
                sb.Append(spaces(indent));
                sb.Append("$ref: ");
                sb.Append(this.GetHashCode());
                return sb.ToString();
            }
            visited.Add(this);
            sb.Append('{');             
            PropertyInfo[] fields = typeof(JSONSchema).GetProperties();
            foreach (PropertyInfo field in fields)
            {
                if (field.GetValue(this) == null) continue;

                sb.AppendLine();
                sb.Append(spaces(indent + 1));
                sb.Append("\"" + field.Name + "\" : ");

                if (field.PropertyType == typeof(string) || field.PropertyType == typeof(UInt32?) || field.PropertyType == typeof(bool?) || field.PropertyType == typeof(SimpleType))
                {

                    sb.Append("\"");
                    sb.Append(field.GetValue(this));
                    sb.Append("\"");
                }

                if (field.PropertyType == typeof(Dictionary<string, JSONSchema>))
                {
                    sb.Append("{");
                    Dictionary<string, JSONSchema> dict = field.GetValue(this) as Dictionary<string, JSONSchema>;
                    int n = dict.Count;
                    int i = 1;
                    foreach (string key in dict.Keys)
                    {
                        sb.AppendLine();
                        sb.Append(spaces(indent + 2));
                        sb.Append("\"" + key + "\": ");
                        sb.Append(dict[key].ToString(indent + 2, visited));
                        if (i < n) sb.Append(',');
                    }
                    sb.AppendLine();
                    sb.Append(spaces(indent + 1) + "}");
                }

                System.Type fieldType = field.PropertyType;
                Object fieldValue = field.GetValue(this);

                if (field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition() == typeof(AnyOf<,>))
                {
                    fieldType = (System.Type)field.PropertyType.GetMethod("GetUnderlyingType").Invoke(field.GetValue(this), null);
                    //sb.Append(fieldType.Name);
                    fieldValue = field.PropertyType.GetMethod("GetValue").Invoke(field.GetValue(this), null);
                }


                if (fieldType == typeof(SimpleType))
                {
                    sb.Append("\"");
                    sb.Append((fieldValue as SimpleType).Value);
                    sb.Append("\"");
                }


                if (fieldType == typeof(SchemaArray))
                {
                    sb.Append("[");
                    SchemaArray arr = fieldValue as SchemaArray;
                    int n = arr.Count;
                    int i = 1;
                    foreach (JSONSchema schema in arr)
                    {
                        sb.AppendLine();
                        sb.Append(spaces(indent + 1));
                        sb.Append(schema.ToString(indent + 1, visited));
                        if (i < n) sb.Append(',');
                    }
                    sb.AppendLine();
                    sb.Append(spaces(indent + 1) + "]");
                }

                if (fieldType == typeof(JSONSchema))
                {
                    sb.Append((fieldValue as JSONSchema).ToString(indent + 1, visited));
                }

                if (fieldType == typeof(UniqueNonEmptyList<JsonElement>))
                {
                    var list = fieldValue as UniqueNonEmptyList<JsonElement>;
                    sb.Append("[ ");
                    int n = list.Count;
                    int i = 1;
                    foreach (JsonElement jsonElement in list)
                    {
                        sb.Append(jsonElement.ToString());
                        if (i < n) sb.Append(", ");
                    }
                    sb.Append(" ]");
                }

                if (fieldType == typeof(JsonElement?))
                {
                    JsonElement jsonElement = (JsonElement)fieldValue;
                    sb.Append(jsonElement.ToString());
                }

            }
            sb.AppendLine();
            sb.Append(spaces(indent));
            sb.Append('}');
            return sb.ToString();
        }

        private StringBuilder spaces(int indent)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                sb.Append("  ");
            }
            return sb;
        }

        private JSONSchema()
        {

        }

        public JSONSchema(JsonDocument schema) : this(schema.RootElement,true)
        {
        }

        public JSONSchema(JsonElement root) : this(root, false) { }

        public JSONSchema(JsonElement root, bool isTopLevel)
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
                    Console.WriteLine($"Invalid JSON Schema. {current.Name} is not a supported JsonSchema property");
                }
            }

            if (isTopLevel)
            {
                ReplaceRefs(this, this);
            }

        }

        private static void ReplaceRefs(JSONSchema schema, JSONSchema root)
        {

            ReplaceRefsInDictionary(schema.Properties, root);
            ReplaceRefsInDictionary(schema.Definitions, root);
            ReplaceRefsInDictionary(schema.PatternProperties, root);

            if (schema.AdditionalProperties != null)
            {
                ReplaceRefsInAnyOf<bool, JSONSchema>(schema.AdditionalProperties, root);
            }
            if (schema.AdditionalItems != null)
            {
                ReplaceRefsInAnyOf<bool, JSONSchema>(schema.AdditionalItems, root);
            }
            if (schema.Items != null)
            {
                ReplaceRefsInAnyOf<JSONSchema, SchemaArray>(schema.Items, root);
            }

            ReplaceRefsInSchemaArray(schema.AnyOf, root);
            ReplaceRefsInSchemaArray(schema.AllOf, root);
            ReplaceRefsInSchemaArray(schema.OneOf, root);

          

            if (schema.Not?.Ref != null)
            {
                schema.Not = convertRefToSchema(schema.Not.Ref, root);
            }


        }

        private static void ReplaceRefsInAnyOf<T1,T2>(AnyOf<T1,T2> anyOf, JSONSchema root)
        {
            if (anyOf?.GetValue() is JSONSchema)
            {
                var anyOfSchema = (JSONSchema)anyOf.GetValue();
                if (anyOfSchema?.Ref != null)
                {
                    anyOf.ChangeValue(convertRefToSchema(anyOfSchema.Ref, root));
                }
                else
                {
                    ReplaceRefs(anyOfSchema, root);
                }
            }

            if (anyOf?.GetValue() is SchemaArray)
            {
                var anyOfSchema = (SchemaArray)anyOf.GetValue();
                ReplaceRefsInSchemaArray(anyOfSchema, root);
            }


        }

        private static void ReplaceRefsInSchemaArray(SchemaArray array, JSONSchema root)
        {
            if (array?.Count > 0)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i]?.Ref != null)
                    {
                        array[i] = convertRefToSchema(array[i].Ref, root);
                    }
                    else
                    {
                        ReplaceRefs(array[i], root);
                    }
                }
            }
        }

        private static void ReplaceRefsInDictionary(Dictionary<string, JSONSchema> dictionary, JSONSchema root)
        {
            if (dictionary != null)
            {
                var keyList = new List<string>(dictionary.Keys);
                foreach (string property in keyList)
                {
                    if (dictionary[property]?.Ref != null)
                    {
                        dictionary[property] = convertRefToSchema(dictionary[property].Ref, root);
                    }
                    else
                    {
                        ReplaceRefs(dictionary[property], root);
                    }
                }
            }
        }

        private static JSONSchema convertRefToSchema(string @ref, JSONSchema root)
        {
            if(@ref.Equals("#"))
            {
                return root;
            }
            else
            {                
                var paths = @ref.Split('/');
                if (paths.Length == 3 && paths[0].Equals("#") && paths[1].Equals("definitions"))
                {
                    return root.Definitions[paths[2]];
                }
                else
                {
                    throw new NotImplementedException("this type of reference is not supported");
                }

            }
        }

        public static JSONSchema MergeSchemas(SchemaArray schemas)
        {
            JSONSchema result = new JSONSchema();

            //handle recursivity
            schemas = new SchemaArray(
                schemas.Where(x => x.AllOf == null || !x.AllOf.Any())
                .Union(
                    schemas.Where(x => x.AllOf != null && x.AllOf.Any())
                    .Select(x => MergeSchemas(x.AllOf))).ToList());

            //This won't raise an exception if different (incompatible) types are defined
            result.Type = schemas.Where(x => x.Type != null).Select(x => x.Type).FirstOrDefault();
            
            var multiples = schemas.Where(x => x.MultipleOf.HasValue).Select(x => x.MultipleOf.Value);
            if (multiples.Any())
            {
                result.MultipleOf = multiples.Aggregate((uint)1, (lcm, next) => LCM(lcm, next));
            }
            
            var max = schemas.Where(x => x.Maximum.HasValue).Select(x => x.Maximum);
            if(max.Any())
            {
                result.Maximum = max.Min();
                result.ExclusiveMaximum = schemas.Where(x => x.ExclusiveMaximum.HasValue && x.Maximum == result.Maximum).Select(x => x.ExclusiveMaximum).FirstOrDefault();
            }

            var min = schemas.Where(x => x.Mininum.HasValue).Select(x => x.Mininum);
            if (min.Any())
            {
                result.Mininum = min.Max();
                result.ExclusiveMinimum = schemas.Where(x => x.ExclusiveMinimum.HasValue && x.Mininum == result.Mininum).Select(x => x.ExclusiveMinimum).FirstOrDefault();
             }

            var maxLength = schemas.Where(x => x.MaxLength.HasValue).Select(x => x.MaxLength);
            if (maxLength.Any())
            {
                result.MaxLength = maxLength.Min();
            }

            var minLength = schemas.Where(x => x.MinLength.HasValue).Select(x => x.MinLength);
            if (minLength.Count() > 0)
            {
                result.MinLength = minLength.Max();
            }

            var patterns = schemas.Where(x => !string.IsNullOrEmpty(x.Pattern)).Select(x => x.Pattern);
            if(patterns.Any())
            {
                result.Pattern = string.Concat(patterns.Select(x => string.Format("(?={0})", x)));
            }
            

            //TODO: items

            var maxItems = schemas.Where(x => x.MaxItems.HasValue).Select(x => x.MaxItems);
            if (maxItems.Any())
            {
                result.MaxItems = maxItems.Min();
            }

            var minItems = schemas.Where(x => x.MinItems.HasValue).Select(x => x.MinItems);
            if (minItems.Any())
            {
                result.MinItems = minItems.Max();
            }

            var properties = schemas.Where(x => x.Properties!=null && x.Properties.Any()).Select(x => x.Properties);
            if (properties.Any())
            {
                result.Properties = properties.SelectMany(dict => dict)
                         .ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());
            }

            return result;

        }
    }

    public class SimpleType
    {
        public const string Array = "array";
        public const string Boolean = "boolean";
        public const string Integer = "integer";
        public const string Null = "null";
        public const string Number = "number";
        public const string Object = "object";
        public const string String = "string";

        static readonly HashSet<string> Values = new HashSet<string>(new string[] { Array, Boolean, Integer, Null, Number, Object, String });

        public string Value
        {
            get;
        }
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

        public override string ToString()
        {
            return Value;
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

        public SchemaArray(List<JSONSchema> schemas)
        {
            this.AddRange(schemas);
        }
    }    
}
