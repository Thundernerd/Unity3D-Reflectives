using System;
using System.CodeDom;
using System.Linq;
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
            else if (type.IsClass || type.IsInterface)
            {
                declaration = ClassExporter.Generate(type);
            }

            Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => !x.Name.Any(y => y == '<' || y == '>'))
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.IsSubclassOf(typeof(Delegate)))
                .ToArray();

            foreach (Type nestedType in nestedTypes)
            {
                CodeTypeDeclaration nestedDeclaration = Export(nestedType);
                declaration.Members.Add(nestedDeclaration);
            }

            return declaration;
        }
    }
}
