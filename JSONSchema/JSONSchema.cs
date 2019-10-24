using System;
using System.Collections.Generic;

namespace JSONSchema2POCO
{
    public class JSONSchema
    {
        public string Id { get; set; }

        public string Schema { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public OneOf<JSONSchema, SchemaArray > items;

        public class SchemaArray : List<JSONSchema>
        {

        }
    }
}
