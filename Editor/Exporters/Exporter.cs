using System;
using System.CodeDom;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class Exporter
    {
        public const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        
        public static CodeTypeDeclaration Export(Type type)
        {
            CodeTypeDeclaration declaration = null;

            if (type.IsEnum)
            {
                declaration = EnumExporter.Generate(type);
            }
            else if (type.IsClass)
            {
                declaration = ClassExporter.Generate(type);
            }

            Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

            foreach (Type nestedType in nestedTypes)
            {
                CodeTypeDeclaration nestedDeclaration = Export(nestedType);
                declaration.Members.Add(nestedDeclaration);
            }

            return declaration;
        }
    }
}
