using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ClassGenerator
{
    public static class MyTypeBuilder
    {

        public static Type CreateType(string TypeName)
        {
            TypeBuilder tb = GetTypeBuilder(TypeName);
            return tb.CreateType();
        }

        private static TypeBuilder GetTypeBuilder(string TypeName)
        {
            var an = new AssemblyName(TypeName);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(TypeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

    }
}