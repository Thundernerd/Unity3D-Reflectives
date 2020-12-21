using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace TNRD.Reflectives.Exporters
{
    public class PropertyExporter : MemberExporter
    {
        public override void Export(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            ExportProperties(type, definitionWriter, constructionWriter, bodyWriter);
        }

        private void ExportProperties(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            PropertyInfo[] properties = type.GetProperties(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.Name.Contains("<"))
                .ToArray();

            foreach (PropertyInfo property in properties)
            {
                ExportProperty(property, definitionWriter, constructionWriter, bodyWriter);
            }
        }

        private void ExportProperty(PropertyInfo property, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            string typeName = property.PropertyType.GetNiceName().Replace(".", "_");
            string memberName = property.GetNiceName();

            if (IsPublic(property.PropertyType))
            {
                ExportPublicProperty(property, definitionWriter, constructionWriter, bodyWriter, typeName, memberName);
            }
            else
            {
                ExportNonPublicProperty(property, definitionWriter, constructionWriter, bodyWriter, memberName, typeName);
            }
        }

        private void ExportPublicProperty(
            PropertyInfo property,
            IndentedTextWriter definitionWriter,
            IndentedTextWriter constructionWriter,
            IndentedTextWriter bodyWriter,
            string typeName,
            string memberName
        )
        {
            definitionWriter.WriteLine($"private ReflectiveProperty<{typeName}> property_{memberName};");
            constructionWriter.WriteLine($"property_{memberName} = CreateProperty<{typeName}>(\"{memberName}\", {GetBindingFlags(property)});");

            bodyWriter.WriteLine($"public {typeName} {memberName}");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;

            if (property.CanRead)
            {
                bodyWriter.WriteLine($"get => property_{memberName}.GetValue();");
            }

            if (property.CanWrite)
            {
                bodyWriter.WriteLine($"set => property_{memberName}.SetValue(value);");
            }

            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
        }

        private void ExportNonPublicProperty(
            PropertyInfo property,
            IndentedTextWriter definitionWriter,
            IndentedTextWriter constructionWriter,
            IndentedTextWriter bodyWriter,
            string memberName,
            string typeName
        )
        {
            definitionWriter.WriteLine($"private ReflectiveProperty property_{memberName};");
            constructionWriter.WriteLine($"property_{memberName} = CreateProperty(\"{memberName}\", {GetBindingFlags(property)});");

            bodyWriter.WriteLine($"public {typeName} {memberName}");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;

            if (property.CanRead)
            {
                bodyWriter.WriteLine("get");
                bodyWriter.WriteLine("{");
                bodyWriter.Indent++;

                if (property.PropertyType.IsEnum)
                {
                    Type underlyingType = property.PropertyType.GetEnumUnderlyingType();
                    bodyWriter.WriteLine($"object _temp = ({underlyingType.GetNiceName().Replace(".", "_")})property_{memberName}.GetValue();");
                    bodyWriter.WriteLine($"return ({typeName})_temp;");
                }
                else if (IsEnumerable(property.PropertyType))
                {
                    string genericArgumentName = property.PropertyType.GetGenericArguments()[0].GetNiceName();
                    bodyWriter.WriteLine($"object _temp = property_{memberName}.GetValue();");
                    bodyWriter.WriteLine($"return _temp == null ? null : Utilities.GenerateEnumerable<{genericArgumentName}>(_temp);");
                }
                else if (IsDictionary(property.PropertyType))
                {
                    string genericKeyName = property.PropertyType.GetGenericArguments()[0].GetNiceName();
                    string genericValueName = property.PropertyType.GetGenericArguments()[1].GetNiceName();
                    bodyWriter.WriteLine($"object _temp = property_{memberName}.GetValue();");
                    bodyWriter.WriteLine($"return _temp == null ? null : Utilities.GenerateDictionary<{genericKeyName},{genericValueName}>(_temp);");
                }
                else
                {
                    bodyWriter.WriteLine($"object _temp = property_{memberName}.GetValue();");
                    bodyWriter.WriteLine($"return _temp == null ? null : new {typeName}(_temp);");
                }

                bodyWriter.Indent--;
                bodyWriter.WriteLine("}");
            }

            if (property.CanWrite)
            {
                if (property.PropertyType.IsEnum)
                {
                    Type underlyingType = property.PropertyType.GetEnumUnderlyingType();
                    bodyWriter.WriteLine($"set => property_{memberName}.SetValue(({underlyingType.GetNiceName().Replace(".", "_")})value);");
                }
                else if (IsEnumerable(property.PropertyType))
                {
                    // Not supported
                }
                else if (IsDictionary(property.PropertyType))
                {
                    // Not supported
                }
                else
                {
                    bodyWriter.WriteLine($"set => property_{memberName}.SetValue(value.Instance);");
                }
            }

            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
        }
    }
}