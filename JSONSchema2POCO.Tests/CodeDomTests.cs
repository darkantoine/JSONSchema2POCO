using NUnit.Framework;
using JSONSchema2POCO;
using System;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;

namespace JSONSchema2POCO.Tests
{
    public class CodeDomTests
    {

        CodeCompileUnit targetUnit = new CodeCompileUnit();
        CodeNamespace globalnamespace = new CodeNamespace();
        CodeNamespace codeNamespace;
        CodeTypeDeclaration targetClass;

       [SetUp]
        public void Setup()
        {
            codeNamespace = new CodeNamespace("example");
            globalnamespace.Imports.Add(new CodeNamespaceImport("System"));
            targetClass = new CodeTypeDeclaration("GeneratedClass")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };
        }

        [Test]
        public void Test1()
        {
            // Declares a constructor that takes a string argument.
            CodeConstructor stringConstructor = new CodeConstructor();
            stringConstructor.Attributes = MemberAttributes.Private;
            // Declares a parameter of type string named "TestStringParameter".
            //stringConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "TestStringParameter"));
            // Adds the constructor to the Members collection of the BaseType.
            targetClass.Members.Add(stringConstructor);



            codeNamespace.Types.Add(targetClass);
            targetUnit.Namespaces.Add(codeNamespace);
            targetUnit.Namespaces.Add(globalnamespace);

            Console.WriteLine(Print());
        }

        public string Print()
        {

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };

            using StringWriter sourceWriter = new StringWriter();
            provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
            return sourceWriter.ToString();
        }
    }
}