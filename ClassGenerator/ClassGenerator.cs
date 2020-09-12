using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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

        protected static CodeConditionStatement If(CodeExpression condition, params CodeStatement[] trueStatements) {
             return new CodeConditionStatement(condition, trueStatements);
            }

        protected static CodeConditionStatement If(CodeExpression condition, CodeStatement[] trueStatements, CodeStatement[] falseStatements)
        {
            return new CodeConditionStatement(condition, trueStatements, falseStatements);
        }

        protected static CodeBinaryOperatorExpression Compare(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right) {
            return new CodeBinaryOperatorExpression(left, op, right);
        }

        protected static CodePropertySetValueReferenceExpression ValueRef()
        {
            return new CodePropertySetValueReferenceExpression();
        }

        protected static CodeThrowExceptionStatement Throw(Type ExceptionType)
        {
            return new CodeThrowExceptionStatement(
                                new CodeObjectCreateExpression(ExceptionType));
        }

        protected static CodeAssignStatement Assign(CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        protected static CodeFieldReferenceExpression ThisDot(string fieldName)
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
        }
    }   
}
