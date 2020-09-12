﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace example
{


    public class testClass
    {

        public string id;

        public string schema;

        public string title;

        public string description;

        public decimal multipleOf;

        public decimal maximum;

        public bool exclusiveMaximum;

        public decimal minimum;

        public bool exclusiveMinimum;

        public positiveInteger maxLength;

        public positiveIntegerDefault0 minLength;

        public string pattern;

        public positiveInteger maxItems;

        public positiveIntegerDefault0 minItems;

        public bool uniqueItems;

        public positiveInteger maxProperties;

        public positiveIntegerDefault0 minProperties;

        public stringArray required;

        public object definitions;

        public object properties;

        public object patternProperties;

        public object dependencies;

        public System.Collections.IList @enum;

        public string format;

        public schemaArray allOf;

        public schemaArray anyOf;

        public schemaArray oneOf;

        public testClass not;

        public class schemaArray : List<testClass>
        {

            // Hiding visiblity of default constructor as a minimum of 1 elements is required
            private schemaArray()
            {
            }

            public schemaArray(IEnumerable<testClass> collection)
            {
                if (collection.Count() < 1)
                {
                    throw new System.ArgumentException();
                }
            }
        }

        public class positiveInteger
        {

            public int Value
            {
                get
                {
                    return this.Value;
                }
                set
                {
                    if ((value < 0))
                    {
                        throw new System.ArgumentOutOfRangeException();
                    }
                    this.Value = value;
                }
            }
        }

        public class positiveIntegerDefault0
        {
        }

        public class simpleTypes
        {
        }

        public class stringArray : List<String>
        {

            // Hiding visiblity of default constructor as a minimum of 1 elements is required
            private stringArray()
            {
            }

            public stringArray(IEnumerable<String> collection)
            {
                if (collection.Count() < 1)
                {
                    throw new System.ArgumentException();
                }
            }
        }
    }
}