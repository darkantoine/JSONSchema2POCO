using System;
using System.CodeDom;
using System.Collections.Generic;
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
    }
}
