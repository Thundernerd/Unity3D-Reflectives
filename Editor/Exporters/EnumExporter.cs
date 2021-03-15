using System;
using System.CodeDom;

namespace TNRD.Reflectives.Exporters
{
    public class EnumExporter
    {
        public static CodeTypeDeclaration Generate(Type type)
        {
            return new EnumExporter(type).Generate();
        }

        private readonly Type type;

        private EnumExporter(Type type)
        {
            this.type = type;
        }

        private CodeTypeDeclaration Generate()
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(type.Name)
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                IsEnum = true
            };

            declaration.Comments.Add(new CodeCommentStatement($"Original: {type.AssemblyQualifiedName}"));
            declaration.BaseTypes.Add(type.GetEnumUnderlyingType());

            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                Enum value = (Enum) values.GetValue(i);
                object converted = Convert.ChangeType(value, value.GetTypeCode());
                CodeMemberField field = new CodeMemberField(type.Name, name)
                {
                    InitExpression = new CodePrimitiveExpression(converted)
                };
                declaration.Members.Add(field);
            }

            return declaration;
        }
    }
}
