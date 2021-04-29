using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;

namespace TNRD.Reflectives.Exporters
{
    public class ExportUtils
    {
        public static CodeTypeReference GetCodeTypeReference(Type type)
        {
            if (IsPublic(type))
            {
                return new CodeTypeReference(type);
            }

            Type[] genericArguments = type.GetGenericArguments();

            if (IsDictionary(type))
            {
                Type first = genericArguments[0];
                Type second = genericArguments[1];

                CodeTypeReference codeTypeReference = new CodeTypeReference(type.GetGenericTypeDefinition());
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(first));
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(second));

                return codeTypeReference;
            }

            if (IsEnumerable(type) && !type.IsArray)
            {
                Type first = genericArguments[0];

                CodeTypeReference codeTypeReference = new CodeTypeReference(type.GetGenericTypeDefinition());
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(first));

                return codeTypeReference;
            }

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                CodeTypeReference elementTypeReference = GetCodeTypeReference(elementType);
                // Bit of a hack by creating a new CodeDomProvider
                string elementTypeOutput = CodeDomProvider.CreateProvider("CSharp").GetTypeOutput(elementTypeReference);
                CodeTypeReference codeTypeReference = new CodeTypeReference($"{elementTypeOutput}[]");
                return codeTypeReference;
            }

            // Very specific case
            if (type.AssemblyQualifiedName?.StartsWith("System.Nullable") ?? false)
            {
                CodeTypeReference typeReference = new CodeTypeReference(typeof(Nullable<>));
                typeReference.TypeArguments.Add(GetCodeTypeReference(genericArguments[0]));
                return typeReference;
            }

            if (genericArguments.Length > 0)
            {
                CodeTypeReference typeReference = new CodeTypeReference(type.GetGenericTypeDefinition());
                foreach (Type genericArgument in genericArguments)
                {
                    CodeTypeReference genericTypeReference = GetCodeTypeReference(genericArgument);
                    typeReference.TypeArguments.Add(genericTypeReference);
                }

                return typeReference;
            }

            return new CodeTypeReference(type.Name);
        }

        public static bool IsPublic(Type type)
        {
            return Utilities.IsPublic(type);
        }

        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }
    }
}
