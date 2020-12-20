using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace TNRD.Reflectives.Exporters
{
    public class FieldExporter : MemberExporter
    {
        public override void Export(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            ExportFields(type, definitionWriter, constructionWriter, bodyWriter);
        }

        private void ExportFields(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            FieldInfo[] fields = type.GetFields(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.Name.Contains("<"))
                .ToArray();

            foreach (FieldInfo field in fields)
            {
                ExportField(field, definitionWriter, constructionWriter, bodyWriter);
            }
        }

        private void ExportField(FieldInfo field, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            string typeName = field.FieldType.GetNiceName();
            string memberName = field.GetNiceName();

            if (IsPublic(field.FieldType))
            {
                ExportPublicField(field, definitionWriter, constructionWriter, bodyWriter, typeName, memberName);
            }
            else
            {
                ExportNonPublicField(field, definitionWriter, constructionWriter, bodyWriter, memberName, typeName);
            }
        }

        private void ExportPublicField(
            FieldInfo field,
            IndentedTextWriter definitionWriter,
            IndentedTextWriter constructionWriter,
            IndentedTextWriter bodyWriter,
            string typeName,
            string memberName
        )
        {
            definitionWriter.WriteLine($"private ReflectiveField<{typeName}> field_{memberName};");
            constructionWriter.WriteLine($"field_{memberName} = CreateField<{typeName}>(\"{memberName}\", {GetBindingFlags(field)});");

            bodyWriter.WriteLine($"public {typeName} {memberName}");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;
            bodyWriter.WriteLine($"get => field_{memberName}.GetValue();");
            bodyWriter.WriteLine($"set => field_{memberName}.SetValue(value);");
            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
        }

        private void ExportNonPublicField(
            FieldInfo field,
            IndentedTextWriter definitionWriter,
            IndentedTextWriter constructionWriter,
            IndentedTextWriter bodyWriter,
            string memberName,
            string typeName
        )
        {
            definitionWriter.WriteLine($"private ReflectiveField field_{memberName};");
            constructionWriter.WriteLine($"field_{memberName} = CreateField(\"{memberName}\", {GetBindingFlags(field)});");

            bodyWriter.WriteLine($"public {typeName} {memberName}");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;
            bodyWriter.WriteLine("get");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;
            if (field.FieldType.IsEnum)
            {
                Type underlyingType = field.FieldType.GetEnumUnderlyingType();
                bodyWriter.WriteLine($"object _temp = ({underlyingType.GetNiceName()})field_{memberName}.GetValue();");
                bodyWriter.WriteLine($"return ({typeName})_temp;");
            }
            else
            {
                bodyWriter.WriteLine($"object _temp = field_{memberName}.GetValue());");
                bodyWriter.WriteLine($"return _temp == null ? null : new {typeName}(_temp);");
            }

            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
            if (field.FieldType.IsEnum)
            {
                Type underlyingType = field.FieldType.GetEnumUnderlyingType();
                bodyWriter.WriteLine($"set => field_{memberName}.SetValue(({underlyingType.GetNiceName()})value.Instance);");
            }
            else
            {
                bodyWriter.WriteLine($"set => field_{memberName}.SetValue(value.Instance);");
            }

            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
        }
    }
}