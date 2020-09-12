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

namespace JSONSchema2POCO
{
    public class ClassGeneratorFromJsonSchema : ClassGenerator
    {
        private readonly JSONSchema schema;

        private readonly Dictionary<JSONSchema, GenerationResult> context;
        private readonly HashSet<string> propertyNames = new HashSet<string>();

        public ClassGeneratorFromJsonSchema(JSONSchema schema, string title = null) : base(title ?? schema.Title)
        {
            this.schema = schema;
            context = new Dictionary<JSONSchema, GenerationResult>();
        }

        private ClassGeneratorFromJsonSchema(JSONSchema schema, Dictionary<JSONSchema, GenerationResult> context, string title = null) : base( title ?? schema.Title)
        {
            this.schema = schema;
            this.context = context;
        }

        private CodeTypeDeclaration GenerateClass()
        {
            context.Add(schema, new GenerationResult() { TypeName = targetClass.Name });
            if (schema.Type?.Value is SimpleType)
            {
                var schemaType = schema.Type.Value as SimpleType;
                if (schemaType.Value == SimpleType.Integer)
                {
                    return GenerateClassFromIntegerSchema();
                }

                if(schemaType.Value == SimpleType.Number)
                {
                    return GenerateClassFromNumberSchema();
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
                        AddNestedClass(nestedClassGenerator.GenerateClass());                        
                    }
                    //context.Add(definition, new GenerationResult() { TypeName = nestedClassGenerator.targetClass.Name });

                }
            }

            var properties = schema.Properties;
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
                    else if (property.Type?.GetUnderlyingType() == typeof(SimpleType))
                    {
                        targetClass.AddProperty( cleanPropertyName, ComputeType((SimpleType)property.Type.GetValue()));
                    }
                }
            }


            return targetClass;
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
                    new CodeSnippetExpression(string.Format("Value % {0} != 0", schema.MultipleOf.Value)),
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
            context[schema].Imports = new List<CodeNamespaceImport>();
            context[schema].Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            var itemsSchema = schema.Items.Value as JSONSchema;
            string itemsSchemaType;
            if (context.ContainsKey(itemsSchema))
            {
                itemsSchemaType = context[itemsSchema].TypeName;
            }
            else
            {
                itemsSchemaType = ComputeType(itemsSchema);
                
            }
            targetClass.BaseTypes.Add(new CodeTypeReference("List<" + itemsSchemaType + ">"));
            this.globalnamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

            if (schema.MinItems.HasValue && schema.MinItems.Value>0)
            {
                CodeConstructor constructor = new CodeConstructor
                {
                    Attributes = MemberAttributes.Private
                };
                constructor.Comments.Add(new CodeCommentStatement(
                    string.Format("Hiding visiblity of default constructor as a minimum of {0} elements is required", schema.MinItems.Value)));
                targetClass.Members.Add(constructor);

                CodeConstructor publicConstructor = new CodeConstructor
                {
                    Attributes = MemberAttributes.Public
                };
                publicConstructor.Parameters.Add(new CodeParameterDeclarationExpression("IEnumerable<"+ itemsSchemaType + ">", "collection"));
                publicConstructor.Statements.Add(If(new CodeSnippetExpression(string.Format("collection.Count() < {0}", schema.MinItems.Value)), Throw(typeof(ArgumentException))));
                    targetClass.Members.Add(publicConstructor);

                context[schema].Imports.Add(new CodeNamespaceImport("System.Linq"));
            }

            return targetClass;
        }

        private string Clean(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");            
            return UppercaseFirst(rgx.Replace(str, "")); 
        }

        public new void Generate()
        {
            GenerateClass();
            HashSet<CodeNamespaceImport> importSet = new HashSet<CodeNamespaceImport>();
            foreach(var result in context.Values)
            {
                if (result.Imports != null)
                {
                    importSet.UnionWith(result.Imports);
                }
            }
            this.globalnamespace.Imports.AddRange(importSet.ToArray());
            base.Generate();
        }

        private string ComputeType(JSONSchema schema)
        {
            if(schema.Type?.GetUnderlyingType() == typeof(SimpleType))
            {
                var simpleType = schema.Type.Value as SimpleType;
                switch (simpleType.Value)
                {
                    case SimpleType.Boolean:
                        return typeof(Boolean).Name;
                    case SimpleType.String:
                        return typeof(string).Name;
                    case SimpleType.Integer:
                        return typeof(Int32).Name;
                    case SimpleType.Number:
                        return typeof(Decimal).Name;
                    case SimpleType.Object:
                        return typeof(object).Name;
                    case SimpleType.Array:
                        if(schema.Items?.Value is JSONSchema)
                        {
                            var itemsSchema = schema.Items.Value as JSONSchema;
                            if (context.ContainsKey(itemsSchema))
                            {
                                return "List<"+context[itemsSchema].TypeName+">";
                            }
                            else
                            {
                                return "List<" + ComputeType(itemsSchema) + ">";
                            }
                        }
                        return typeof(IList).Name;
                    default:
                        return "object";
                }
            }

            return typeof(object).Name;
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
            public List<CodeNamespaceImport> Imports { get; set; }
            public string TypeName { get; set; }
        }
    }
}
