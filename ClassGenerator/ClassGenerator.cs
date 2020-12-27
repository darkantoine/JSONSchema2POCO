using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using static ClassGenerator.StringUtils;

namespace JSONSchema2POCO
{
    public class ClassGenerator
    {
        protected CodeCompileUnit targetUnit = new CodeCompileUnit();
        protected CodeNamespace globalnamespace = new CodeNamespace();
        protected CodeNamespace codeNamespace;
        protected CodeTypeDeclaration targetClass;        

        public ClassGenerator(string className)
        {
            codeNamespace = new CodeNamespace("example");
            globalnamespace.Imports.Add(new CodeNamespaceImport("System"));
            //Title should be converted to a well formed string
            targetClass = new CodeTypeDeclaration(UppercaseFirst(className) ?? "GeneratedClass")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };
        }

        public void AddNestedClass(CodeTypeDeclaration nestedClass)
        {
            targetClass.Members.Add(nestedClass);
        }

        protected void Generate()
        {

            codeNamespace.Types.Add(targetClass);
            targetUnit.Namespaces.Add(codeNamespace);
            targetUnit.Namespaces.Add(globalnamespace);
        }

        public string Print() { 

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };

            using StringWriter sourceWriter = new StringWriter();
            provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
            return sourceWriter.ToString();
        }

        protected string ConvertToIdentifier(string s)
        {
            if(string.IsNullOrWhiteSpace(s))
            {
                return "_";
            }
            StringBuilder sb = new StringBuilder();
            if (!IsIdentifierStartCharacter(s[0]) || IsKeyword(s))
            {
                sb.Append('_');
            }
            foreach(char c in s)
            {
                if (IsIdentifierPartCharacter(c))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("_");
                }
            }
            return sb.ToString();
        }

    }   
}
