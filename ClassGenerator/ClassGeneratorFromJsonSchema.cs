using ClassGenerator;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static ClassGenerator.CodeTypeDeclarationExtensions;
using static ClassGenerator.CodeDomUtils;
using static ClassGenerator.StringUtils;
using System.Reflection;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.IO;
using System.CodeDom.Compiler;

namespace JSONSchema2POCO
{
    public class ClassGeneratorFromJsonSchema : ClassGenerator
    {
        private readonly JSONSchema schema;

        private readonly Dictionary<JSONSchema, GenerationResult> context;
        private readonly HashSet<string> propertyNames = new HashSet<string>();
        private readonly static Type NumberDefaultType = typeof(double);

        public ClassGeneratorFromJsonSchema(JSONSchema schema, string title = null) : base(title ?? schema.Title)
        {
            this.schema = schema;
            context = new Dictionary<JSONSchema, GenerationResult>();
        }

        private ClassGeneratorFromJsonSchema(JSONSchema schema, Dictionary<JSONSchema, GenerationResult> context, string title = null) : base(title ?? schema.Title)
        {
            this.schema = schema;
            this.context = context;
        }

        private CodeTypeDeclaration GenerateClass()
        {
            if (!context.ContainsKey(schema))
            {

                context.Add(schema, new GenerationResult()
                {
                    TypeName = targetClass.Name,
                    Type = MyTypeBuilder.CreateType(targetClass.Name),
                    ClassGenerator = this
                });
            }

            if (schema.Enum?.Count > 0)
            {
                return GenerateClassFromEnumSchema();
            }

            if (schema.Type?.Value is SimpleType && (schema.Type.Value as SimpleType).Value != SimpleType.Object)
            {
                var schemaType = schema.Type.Value as SimpleType;
                if (schemaType.Value == SimpleType.Integer)
                {
                    return GenerateClassFromIntegerSchema();
                }

                if (schemaType.Value == SimpleType.Number)
                {
                    return GenerateClassFromNumberSchema();
                }

                if (schemaType.Value == SimpleType.String)
                {
                    return GenerateClassFromStringSchema();
                }

                if (schemaType.Value == SimpleType.Array)
                {
                    return GenerateClassFromArraySchema();
                }

            }
            var definitions = schema.Definitions;
            if (definitions != null)
            {
                foreach (string definitionName in schema.Definitions.Keys)
                {
                    var definition = definitions[definitionName];
                    if (!context.ContainsKey(definition))
                    {
                        var nestedClassGenerator = new ClassGeneratorFromJsonSchema(definition, context);
                        context[definition] = new GenerationResult()
                        {
                            Type = MyTypeBuilder.CreateType(nestedClassGenerator.targetClass.Name),
                            TypeName = nestedClassGenerator.targetClass.Name,
                            ClassGenerator = nestedClassGenerator
                        };
                        nestedClassGenerator.GenerateClass();
                    }
                    //context.Add(definition, new GenerationResult() { TypeName = nestedClassGenerator.targetClass.Name });

                }
            }

            var properties = schema.Properties;
            var additionalProperties = schema.AdditionalProperties;

            //Oneof/AnyOf/AllOf are only supported when there are no properties or additionalProperties and when the schema type is not a primitive type or array type
            if (properties == null && additionalProperties == null)
            {
                if (schema.OneOf != null && schema.OneOf.Count > 0)
                {
                    return GenerateClassFromOneOfAnyOfSchema(true);
                }

                if (schema.AnyOf != null && schema.AnyOf.Count > 0)
                {
                    return GenerateClassFromOneOfAnyOfSchema(false);
                }

                if (schema.AllOf != null && schema.AllOf.Count > 0)
                {
                    var mergedSchema = JSONSchema.MergeSchemas(schema.AllOf);
                    var mergedClassGenerator = new ClassGeneratorFromJsonSchema(mergedSchema,this.targetClass.Name);
                    targetClass = mergedClassGenerator.GenerateClass();
                    context[schema] = mergedClassGenerator.context[mergedSchema];
                    foreach (var jsonSchema in mergedClassGenerator.context.Keys)
                    {
                        if (jsonSchema != mergedSchema)
                        {
                            context[jsonSchema] = mergedClassGenerator.context[jsonSchema];
                        }
                    }
                    return targetClass;

                }

            }
            
            if (properties != null)
            {
                foreach (string propertyName in properties.Keys)
                {
                    var cleanPropertyName = Clean(propertyName);
                    if (propertyNames.Contains(cleanPropertyName))
                    {
                        //to avoid property names that would collide
                        continue;
                    }
                    propertyNames.Add(cleanPropertyName);
                    var property = properties[propertyName];

                    if (context.ContainsKey(property))
                    {
                        targetClass.AddProperty(cleanPropertyName, context[property].TypeName);
                    }
                    else if (property.Type?.GetUnderlyingType() == typeof(SimpleType)
                        && (property.Type.Value as SimpleType).Value != SimpleType.Object
                        && (property.Type.Value as SimpleType).Value != SimpleType.Array
                        && (property.Enum == null || property.Enum.Count < 1)
                        )
                    {
                        targetClass.AddProperty(cleanPropertyName, ComputeType((SimpleType)property.Type.GetValue()));
                    }
                    else
                    {
                        var nestedClassGenerator = new ClassGeneratorFromJsonSchema(property, context, cleanPropertyName);
                        context[property] = new GenerationResult()
                        {
                            Type = MyTypeBuilder.CreateType(nestedClassGenerator.targetClass.Name),
                            TypeName = nestedClassGenerator.targetClass.Name,
                            ClassGenerator = nestedClassGenerator
                        };
                        nestedClassGenerator.GenerateClass();
                        targetClass.AddProperty(cleanPropertyName, context[property].Type);
                    }
                }
            }

            
            if (additionalProperties != null)
            {
                if (additionalProperties.Value is bool)
                {
                    if ((bool)additionalProperties.Value == true)
                    {
                        targetClass.BaseTypes.Add(new CodeTypeReference(typeof(Dictionary<string, object>)));
                    }
                }
                else
                {
                    var additionalPropertiesSchema = additionalProperties.Value as JSONSchema;
                    if (!context.ContainsKey(additionalPropertiesSchema))
                    {
                        var nestedClassGenerator = new ClassGeneratorFromJsonSchema(additionalPropertiesSchema, context, context[schema].TypeName + "AdditionalProperties");
                        context[additionalPropertiesSchema] = new GenerationResult()
                        {
                            Type = MyTypeBuilder.CreateType(nestedClassGenerator.targetClass.Name),
                            TypeName = nestedClassGenerator.targetClass.Name,
                            ClassGenerator = nestedClassGenerator
                        };
                        nestedClassGenerator.GenerateClass();
                    }
                    targetClass.BaseTypes.Add(new CodeTypeReference("Dictionary", new CodeTypeReference(typeof(string)), new CodeTypeReference(context[additionalPropertiesSchema].Type)));
                    context[schema].Imports.Add("System.Collections.Generic");
                }
            }

            

            return targetClass;
        }

        private CodeTypeDeclaration GenerateClassFromStringSchema()
        {
            CodeMemberProperty property = NewProperty("Value", typeof(string), false) as CodeMemberProperty;
            targetClass.AddProperty(property);
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Value")));
            if (schema.MinLength.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        new CodeFieldReferenceExpression(ValueRef(), "Length"),
                        CodeBinaryOperatorType.LessThan,
                        new CodePrimitiveExpression((int)schema.MinLength.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (schema.MaxLength.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        new CodeFieldReferenceExpression(ValueRef(),"Length"),
                        CodeBinaryOperatorType.GreaterThan,
                        new CodePrimitiveExpression((int)schema.MaxLength.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (!string.IsNullOrEmpty(schema.Pattern))
            {
                context[schema].Imports.Add("System.Text.RegularExpressions");
                property.SetStatements.Add(
                    new CodeVariableDeclarationStatement("Regex", "regex", new CodeObjectCreateExpression("Regex", Array(new CodeSnippetExpression("@"+ToLiteral(schema.Pattern))))));
                property.SetStatements.Add(
                    If(
                    new CodeSnippetExpression("!regex.IsMatch(value)"),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            property.SetStatements.Add(Assign(ThisDot("Value"), ValueRef()));
            return targetClass;
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        private CodeTypeDeclaration GenerateClassFromOneOfAnyOfSchema(bool handleOneOf)
        {
            ISet<string> subSchemasSet = new HashSet<string>();
            int index = 0;
            var schemaArray = handleOneOf ? schema.OneOf : schema.AnyOf;
            foreach (JSONSchema subSchema in schemaArray)
            {
                if (context.ContainsKey(subSchema))
                {
                    subSchemasSet.Add(context[subSchema].TypeName);
                }
                else if (subSchema.Type?.GetUnderlyingType() == typeof(SimpleType)
                    && (subSchema.Type.Value as SimpleType).Value != SimpleType.Object
                    && (subSchema.Type.Value as SimpleType).Value != SimpleType.Array
                    )
                {
                    subSchemasSet.Add(ComputeType((SimpleType)subSchema.Type.GetValue()).Name);
                }
                else
                {
                    var nestedClassGenerator = new ClassGeneratorFromJsonSchema(subSchema, context, string.Concat(targetClass.Name, handleOneOf?"OneOf":"AnyOf", index));
                    context[subSchema] = new GenerationResult()
                    {
                        Type = MyTypeBuilder.CreateType(nestedClassGenerator.targetClass.Name),
                        TypeName = nestedClassGenerator.targetClass.Name,
                        ClassGenerator = nestedClassGenerator
                    };
                    nestedClassGenerator.GenerateClass();
                    subSchemasSet.Add(nestedClassGenerator.targetClass.Name);
                }
                index++;
            }


            targetClass = new CodeTypeDeclaration(targetClass.Name)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };

            var field = new CodeMemberField()
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name = "Value",
                Type = new CodeTypeReference(typeof(object))
            };
            targetClass.Members.Add(field);

            CodeConstructor constructor = new CodeConstructor
            {
                Attributes = MemberAttributes.Private
            };
            constructor.Comments.Add(new CodeCommentStatement("Hiding visiblity of default constructor"));
            targetClass.Members.Add(constructor);

            CodeStatement[] codeStatements = new CodeStatement[subSchemasSet.Count+1];
            index = 0;
            foreach(string subSchemaName in subSchemasSet)
            {
                codeStatements[index] = If(NewSnippet("value is " + subSchemaName), new CodeAssignStatement(new CodeVariableReferenceExpression("Value"), new CodeVariableReferenceExpression("value")));
                index++;
            }
            codeStatements[index] = Throw(typeof(ArgumentException), "Value's type is not correct");

            CodeConstructor publicConstructor = NewPublicConstructor(
                Array(NewParameter(typeof(object), "value")),
                codeStatements
                );
            targetClass.Members.Add(publicConstructor);

            foreach(string subSchemaName in subSchemasSet)
            {
                var implicitOperator = new CodeSnippetTypeMember(string.Format("\t\tpublic static implicit operator {0}({1} d) => ({0})d.Value;", subSchemaName, targetClass.Name));
                targetClass.Members.Add(implicitOperator);

                var explicitOperator = new CodeSnippetTypeMember(string.Format("\t\tpublic static explicit operator {0}({1} v) => new {0}(v);", targetClass.Name, subSchemaName));
                targetClass.Members.Add(explicitOperator);
            }

            return targetClass;
        }

        private CodeTypeDeclaration GenerateClassFromEnumSchema()
        {
            context[schema].Imports.Add("System.Collections.Generic");
            targetClass = new CodeTypeDeclaration(targetClass.Name)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };


            Type enumType = GetEnumType(schema);
            string enumTypeString = GetTypeString(enumType);

            int i = 1;
            foreach (JsonElement enumValue in schema.Enum)
            {
                CodeMemberField f = new CodeMemberField()
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Static,
                    InitExpression = new CodePrimitiveExpression(GetValue(enumValue)),
                    Name = ConvertToIdentifier(enumValue.ToString()),
                    Type = new CodeTypeReference("readonly " + enumTypeString)
                };
                i++;
                targetClass.Members.Add(f);
            }

            var fieldType = new CodeTypeReference("HashSet", new CodeTypeReference(enumType));
            CodeMemberField field = new CodeMemberField()
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static,
                Name = "Constants",
                Type = fieldType,
                InitExpression = new CodeObjectCreateExpression(fieldType, new CodeArrayCreateExpression(enumType, schema.Enum.Select(value => new CodeVariableReferenceExpression(ConvertToIdentifier(value.ToString()))).ToArray()))
            };
            targetClass.Members.Add(field);

            field = new CodeMemberField()
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name = "Value",
                Type = new CodeTypeReference(enumType)
            };
            targetClass.Members.Add(field);

            CodeConstructor constructor = new CodeConstructor
            {
                Attributes = MemberAttributes.Private
            };
            constructor.Comments.Add(new CodeCommentStatement("Hiding visiblity of default constructor"));
            targetClass.Members.Add(constructor);

            CodeConstructor publicConstructor = NewPublicConstructor(
                Array(NewParameter(enumTypeString, "value")),
                Array(
                    If(
                        NewSnippet("!Constants.Contains(value)"),
                        Throw(typeof(ArgumentException), "Value is not part of enum")
                        ),
                    new CodeAssignStatement(new CodeVariableReferenceExpression("Value"), new CodeVariableReferenceExpression("value"))
                    )
                );
            targetClass.Members.Add(publicConstructor);

            var implicitOperator = new CodeSnippetTypeMember(string.Format("\t\tpublic static implicit operator {0}({1} d) => d.Value;", enumTypeString, targetClass.Name));
            targetClass.Members.Add(implicitOperator);

            var explicitOperator = new CodeSnippetTypeMember(string.Format("\t\tpublic static explicit operator {0}({1} v) => new {0}(v);", targetClass.Name, enumTypeString));
            targetClass.Members.Add(explicitOperator);

            return targetClass;
        }

        private string GetTypeString(Type enumType)
        {
            if (!enumType.IsGenericType)
            {
                return enumType.Name;
            }
            return string.Concat(enumType.Name.Substring(0, enumType.Name.Length - 2), "<", enumType.GenericTypeArguments.First().Name, ">");
        }

        private object GetValue(JsonElement enumValue)
        {
            switch (enumValue.ValueKind)
            {
                case JsonValueKind.String: return enumValue.GetString();
                case JsonValueKind.Number:
                    if (enumValue.TryGetInt32(out Int32 intValue))
                    {
                        return enumValue.GetInt32();
                    }
                    if (enumValue.TryGetInt64(out Int64 LongValue))
                    {
                        return enumValue.GetInt64();
                    }
                    return enumValue.GetDouble();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return enumValue.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                default:
                    return enumValue.ToString();
            }
        }

        private Type GetEnumType(JSONSchema schema)
        {
            if (schema.Type != null && schema.Type.Value is SimpleType)
            {
                return ComputeType(schema);
            }

            if (schema.Type == null)
            {
                HashSet<JsonValueKind> types = new HashSet<JsonValueKind>();
                foreach (JsonElement enumValue in schema.Enum)
                {
                    types.Add(enumValue.ValueKind);
                }
                if (types.Count() == 1)
                {
                    switch (types.First())
                    {
                        case JsonValueKind.String: return typeof(string);
                        case JsonValueKind.Number: return NumberDefaultType;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            return typeof(bool);
                        default:
                            return typeof(object);
                    }
                }
                if (types.Count() == 2)
                {
                    if (types.Contains(JsonValueKind.False) && types.Contains(JsonValueKind.True))
                    {
                        return typeof(bool);
                    }
                    if (types.Contains(JsonValueKind.Null))
                    {
                        switch (types.Where(x => x != JsonValueKind.Null).First())
                        {
                            case JsonValueKind.String: return typeof(string); //strings can null
                            case JsonValueKind.Number: return typeof(Nullable<>).MakeGenericType(NumberDefaultType);
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                return typeof(Nullable<>).MakeGenericType(typeof(bool));
                            default:
                                return typeof(object);//objects can be null
                        }
                    }
                }
                if (types.Count() == 3)
                {
                    if (types.IsSubsetOf(new List<JsonValueKind> { JsonValueKind.Null, JsonValueKind.False, JsonValueKind.True }))
                    {
                        return typeof(Nullable<>).MakeGenericType(typeof(bool));
                    }
                }

            }
            return typeof(object);
        }


        private CodeTypeDeclaration GenerateClassFromNumberSchema()
        {
            CodeMemberProperty property = NewProperty("Value", typeof(float), false) as CodeMemberProperty;
            targetClass.AddProperty(property);
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Value")));
            if (schema.Mininum.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        ValueRef(),
                        (schema.ExclusiveMinimum.HasValue && schema.ExclusiveMinimum.Value) ? CodeBinaryOperatorType.LessThanOrEqual : CodeBinaryOperatorType.LessThan,
                        new CodePrimitiveExpression((int)schema.Mininum.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (schema.Maximum.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        ValueRef(),
                        (schema.ExclusiveMaximum.HasValue && schema.ExclusiveMaximum.Value) ? CodeBinaryOperatorType.GreaterThanOrEqual : CodeBinaryOperatorType.GreaterThan,
                        new CodePrimitiveExpression((int)schema.Maximum.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (schema.MultipleOf.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    new CodeSnippetExpression(string.Format("value % {0} != 0", schema.MultipleOf.Value)),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            property.SetStatements.Add(Assign(ThisDot("Value"), ValueRef()));
            return targetClass;
        }

        private CodeTypeDeclaration GenerateClassFromIntegerSchema()
        {
            CodeMemberProperty property = NewProperty("Value", typeof(int), false) as CodeMemberProperty;
            targetClass.AddProperty(property);
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Value")));
            if (schema.Mininum.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        ValueRef(),
                        (schema.ExclusiveMinimum.HasValue && schema.ExclusiveMinimum.Value) ? CodeBinaryOperatorType.LessThanOrEqual : CodeBinaryOperatorType.LessThan,
                        new CodePrimitiveExpression((int)schema.Mininum.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (schema.Maximum.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    Compare(
                        ValueRef(),
                        (schema.ExclusiveMaximum.HasValue && schema.ExclusiveMaximum.Value) ? CodeBinaryOperatorType.GreaterThanOrEqual : CodeBinaryOperatorType.GreaterThan,
                        new CodePrimitiveExpression((int)schema.Maximum.Value)
                        ),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            if (schema.MultipleOf.HasValue)
            {
                property.SetStatements.Add(
                    If(
                    new CodeSnippetExpression(string.Format("Value % {0} != 0", schema.MultipleOf.Value)),
                    Throw(typeof(ArgumentOutOfRangeException)
                    )));
            }
            property.SetStatements.Add(Assign(ThisDot("Value"), ValueRef()));
            return targetClass;
        }

        private CodeTypeDeclaration GenerateClassFromArraySchema()
        {
            context[schema].Imports.Add("System.Collections.Generic");
            Type itemsSchemaType = typeof(object);
            if (schema.Items != null)
            {
                var itemsSchema = schema.Items.Value as JSONSchema;
                itemsSchemaType = context.ContainsKey(itemsSchema) ?
                    context[itemsSchema].Type : ComputeType(itemsSchema);
            }

            targetClass.BaseTypes.Add(new CodeTypeReference("List", new CodeTypeReference(itemsSchemaType)));

            if (schema.MinItems.HasValue && schema.MinItems.Value > 0)
            {
                CodeConstructor constructor = new CodeConstructor
                {
                    Attributes = MemberAttributes.Private
                };
                constructor.Comments.Add(new CodeCommentStatement(
                    string.Format("Hiding visiblity of default constructor as a minimum of {0} elements is required", schema.MinItems.Value)));
                targetClass.Members.Add(constructor);

                CodeConstructor publicConstructor = NewPublicConstructor(
                    Array(NewParameter("IEnumerable<" + itemsSchemaType.Name + ">", "collection")),
                    Array(
                        If(
                            NewSnippet("collection.Count() < {0}", schema.MinItems.Value),
                            Throw(typeof(ArgumentException))
                            )
                        )
                    );
                targetClass.Members.Add(publicConstructor);

                context[schema].Imports.Add("System.Linq");
            }

            return targetClass;
        }

        private string Clean(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return UppercaseFirst(rgx.Replace(str, ""));
        }

        public void GenerateAll()
        {
            this.GenerateClass();
            foreach (var result in context.Values)
            {
                if (result.Imports.Any())
                {
                    var codeNamespace = new CodeNamespace();
                    codeNamespace.Imports.AddRange(result.Imports.Select(x => new CodeNamespaceImport(x)).ToArray());
                    result.ClassGenerator.targetUnit.Namespaces.Add(codeNamespace);
                }
                result.ClassGenerator.Generate();
            }
        }

        public Dictionary<string, string> PrintAll()
        {
            var results = new Dictionary<string, string>();
            foreach (var result in context.Values)
            {
                results.Add(result.TypeName, result.ClassGenerator.Print());

            }
            return results;
        }

        private Type ComputeType(JSONSchema schema)
        {
            if (schema.Type?.GetUnderlyingType() == typeof(SimpleType))
            {
                var simpleType = schema.Type.Value as SimpleType;
                switch (simpleType.Value)
                {
                    case SimpleType.Boolean:
                        return typeof(Boolean);
                    case SimpleType.String:
                        return typeof(string);
                    case SimpleType.Integer:
                        return typeof(Int32);
                    case SimpleType.Number:
                        return typeof(Decimal);
                    case SimpleType.Object:
                        return typeof(object);
                    case SimpleType.Array:
                        if (schema.Items?.Value is JSONSchema)
                        {
                            var itemsSchema = schema.Items.Value as JSONSchema;
                            if (context.ContainsKey(itemsSchema))
                            {
                                return typeof(List<>).MakeGenericType(context[itemsSchema].Type);
                            }
                            else
                            {
                                return typeof(List<>).MakeGenericType(ComputeType(itemsSchema));
                            }
                        }
                        return typeof(IList);
                    default:
                        return typeof(object);
                }
            }

            return typeof(object);
        }

        private Type ComputeType(SimpleType type)
        {
            switch (type.Value)
            {
                case SimpleType.Boolean:
                    return typeof(Boolean);
                case SimpleType.String:
                    return typeof(string);
                case SimpleType.Integer:
                    return typeof(Int32);
                case SimpleType.Number:
                    return typeof(Decimal);
                case SimpleType.Object:
                    return typeof(object);
                case SimpleType.Array:
                    return typeof(IList);
                default:
                    return null;
            }
        }

        private class GenerationResult
        {
            public HashSet<string> Imports { get; set; } = new HashSet<string>();
            public string TypeName { get; set; }
            public Type Type { get; internal set; }

            public ClassGeneratorFromJsonSchema ClassGenerator { get; set; }
        }
    }
}
