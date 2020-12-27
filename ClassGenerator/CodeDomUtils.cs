using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ClassGenerator
{
    static class CodeDomUtils
    {
        public static CodeTypeMember NewProperty(string propertyName, Type propertyType, bool defaultGetSet = true)
        {
            if (defaultGetSet)
            {
                return CreateNewField(propertyName, propertyType);
            }
            else
            {
                return CreateNewProperty(propertyName, propertyType);
            }
        }

        public static CodeMemberProperty CreateNewProperty(string propertyName, string propertyTypeString)
        {
            return CreateNewProperty(propertyName, null, propertyTypeString);
        }

        public static CodeMemberProperty CreateNewProperty(string propertyName, Type propertyType, string propertyTypeString = null)
        {
            if (propertyType == null && propertyTypeString == null)
            {
                throw new ArgumentNullException("PropertyType must be passed");
            }

            CodeMemberProperty property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = propertyName
            };
            property.Type = propertyType != null ? new CodeTypeReference(propertyType) : new CodeTypeReference(propertyTypeString);

            return property;
        }

        public static CodeMemberField CreateNewField(string propertyName, string propertyTypeString)
        {
            return CreateNewField(propertyName, null, propertyTypeString);
        }

        public static CodeMemberField CreateNewField(string propertyName, Type propertyType, string propertyTypeString = null)
        {
            if (propertyType == null && propertyTypeString == null)
            {
                throw new ArgumentNullException("PropertyType must be passed");
            }

            CodeMemberField field = new CodeMemberField
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = propertyName
            };
            field.Type = propertyType != null ? new CodeTypeReference(propertyType) : new CodeTypeReference(propertyTypeString);

            return field;
        }

        public static CodeConstructor NewPublicConstructor(CodeParameterDeclarationExpression[] parameters, CodeStatement[] statements)
        {
            CodeConstructor result = new CodeConstructor
            {
                Attributes = MemberAttributes.Public
            };
            foreach (var parameter in parameters)
            {
                result.Parameters.Add(parameter);
            }
            foreach(var statement in statements)
            {
                result.Statements.Add(statement);
            }
            return result;
        }

        public static CodeParameterDeclarationExpression NewParameter(string type, string name)
        {
            return new CodeParameterDeclarationExpression(type, name);
        }

        public static CodeParameterDeclarationExpression NewParameter(Type type, string name)
        {
            return new CodeParameterDeclarationExpression(type, name);
        }

        public static CodeStatement[] Array(params CodeStatement[] parameters)
        {
            return parameters;
        }

        public static CodeExpression[] Array(params CodeExpression[] parameters)
        {
            return parameters;
        }

        public static CodeParameterDeclarationExpression[] Array(params CodeParameterDeclarationExpression[] parameters)
        {
            return parameters;
        }

        public static CodeSnippetExpression NewSnippet(String snippet, params object?[] args)
        {
            return new CodeSnippetExpression(string.Format(snippet, args));
        }

        public static CodeConditionStatement If(CodeExpression condition, params CodeStatement[] trueStatements)
        {
            return new CodeConditionStatement(condition, trueStatements);
        }

        public static CodeConditionStatement If(CodeExpression condition, CodeStatement[] trueStatements, CodeStatement[] falseStatements)
        {
            return new CodeConditionStatement(condition, trueStatements, falseStatements);
        }
        public static CodeBinaryOperatorExpression Compare(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
        {
            return new CodeBinaryOperatorExpression(left, op, right);
        }

        public static CodePropertySetValueReferenceExpression ValueRef()
        {
            return new CodePropertySetValueReferenceExpression();
        }

        public static CodeThrowExceptionStatement Throw(Type ExceptionType)
        {
            return new CodeThrowExceptionStatement(
                                new CodeObjectCreateExpression(ExceptionType));
        }

        public static CodeThrowExceptionStatement Throw(Type ExceptionType, string message)
        {
            return new CodeThrowExceptionStatement(
                                new CodeObjectCreateExpression(ExceptionType, new CodePrimitiveExpression(message)));
        }

        public static CodeAssignStatement Assign(CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        public static CodeFieldReferenceExpression ThisDot(string fieldName)
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
        }
    }
}
