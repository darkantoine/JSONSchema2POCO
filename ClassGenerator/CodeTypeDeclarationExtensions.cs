using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using static ClassGenerator.CodeDomUtils;

namespace ClassGenerator
{
    static class CodeTypeDeclarationExtensions
    {
        public static CodeTypeDeclaration AddProperty(this CodeTypeDeclaration targetClass, string propertyName, Type propertyType, bool defaultGetSet = true)
        {
            CodeTypeMember property = NewProperty(propertyName, propertyType, defaultGetSet);
            targetClass.Members.Add(property);
            return targetClass;
        }

        public static CodeTypeDeclaration AddProperty(this CodeTypeDeclaration targetClass, CodeTypeMember property)
        {            
            targetClass.Members.Add(property);
            return targetClass;
        }

               public static CodeTypeMember AddProperty(this CodeTypeDeclaration targetClass, string propertyName, string propertyType, bool defaultGetSet = true)
        {
            CodeTypeMember field = defaultGetSet ? (CodeTypeMember)CreateNewField(propertyName, propertyType) : (CodeTypeMember)CreateNewProperty(propertyName, propertyType);
            targetClass.Members.Add(field);
            return field;
        }

        public static CodeTypeDeclaration AddNestedClass(this CodeTypeDeclaration targetClass, CodeTypeDeclaration nestedClass)
        {
            targetClass.Members.Add(nestedClass);
            return nestedClass;
        }
       
    }
}
