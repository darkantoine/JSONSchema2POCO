using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace ClassGenerator
{
    public static class CodeDomExtensions
    {
        public static void Add(this CodeNamespaceImportCollection imports, string import)
        {
            imports.Add(new CodeNamespaceImport(import));
        }
    }
}
