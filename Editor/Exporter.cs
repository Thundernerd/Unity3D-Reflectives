using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using TNRD.Reflectives.Exporters;

namespace TNRD.Reflectives
{
    internal class Exporter
    {
        public const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly string @namespace;
        private readonly string outputDirectory;

        public Exporter(string @namespace, string outputDirectory)
        {
            this.@namespace = @namespace;
            this.outputDirectory = outputDirectory;
        }

        public void Export(Type type)
        {
            string output = string.Empty;

            if (type.IsEnum)
            {
                output = ExportEnum(type);
            }
            else
            {
                output = ExportClass(type);
            }

            string sanitizedFilename = $"{type.GetNiceName().Replace(".", "_")}.Generated.cs";
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (char invalidFileNameChar in invalidFileNameChars)
            {
                while (sanitizedFilename.Contains(invalidFileNameChar))
                {
                    sanitizedFilename = sanitizedFilename.Replace(invalidFileNameChar.ToString(), string.Empty);
                }
            }

            string outputPath = Path.Combine(outputDirectory, sanitizedFilename);
            
            File.WriteAllText(outputPath, output);
        }

        private string ExportEnum(Type type)
        {
            Type underlyingType = type.GetEnumUnderlyingType();
            StringBuilder builder = new StringBuilder();
            IndentedTextWriter writer = new IndentedTextWriter(new StringWriter(builder), "\t");

            if (!string.IsNullOrEmpty(@namespace))
            {
                writer.WriteLine($"namespace {@namespace}");
                writer.WriteLine("{");
                writer.Indent++;
            }

            writer.WriteLine($"public enum {type.GetNiceName().Replace(".", "_")} : {underlyingType.GetNiceName()}");
            writer.WriteLine("{");
            writer.Indent++;

            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                object value = Convert.ChangeType(values.GetValue(i), underlyingType);
                string suffix = i < names.Length - 1 ? "," : string.Empty;
                writer.WriteLine($"{name}={value}{suffix}");
            }

            writer.Indent--;
            writer.WriteLine("}");

            if (!string.IsNullOrEmpty(@namespace))
            {
                writer.Indent--;
                writer.WriteLine("}");
            }

            return builder.ToString();
        }

        private string ExportClass(Type type)
        {
            StringBuilder definitionBuilder = new StringBuilder();
            StringBuilder constructionBuilder = new StringBuilder();
            StringBuilder bodyBuilder = new StringBuilder();

            IndentedTextWriter definitionWriter = new IndentedTextWriter(new StringWriter(definitionBuilder), "\t");
            IndentedTextWriter constructionWriter = new IndentedTextWriter(new StringWriter(constructionBuilder), "\t");
            IndentedTextWriter bodyWriter = new IndentedTextWriter(new StringWriter(bodyBuilder), "\t");

            definitionWriter.Indent = 1;
            constructionWriter.Indent = 2;
            bodyWriter.Indent = 1;

            if (!string.IsNullOrEmpty(@namespace))
            {
                definitionWriter.Indent++;
                constructionWriter.Indent++;
                bodyWriter.Indent++;
            }

            new EventExporter()
                .Export(type, definitionWriter, constructionWriter, bodyWriter);
            new FieldExporter()
                .Export(type, definitionWriter, constructionWriter, bodyWriter);
            new PropertyExporter()
                .Export(type, definitionWriter, constructionWriter, bodyWriter);
            new MethodExporter()
                .Export(type, definitionWriter, constructionWriter, bodyWriter);

            StringBuilder builder = new StringBuilder();

            WriteHeader(builder, type);
            WriteNamespaces(builder, type);

            IndentedTextWriter mainWriter = new IndentedTextWriter(new StringWriter(builder), "\t");
            bool hasNamespace = !string.IsNullOrEmpty(@namespace);

            if (hasNamespace)
            {
                mainWriter.WriteLine($"namespace {@namespace}");
                mainWriter.WriteLine("{");
                mainWriter.Indent++;
            }

            mainWriter.WriteLine($"public sealed partial class {type.Name} : ReflectiveClass");
            mainWriter.WriteLine("{");
            mainWriter.Indent++;
            mainWriter.WriteLine(definitionBuilder.ToString().TrimEnd());
            WriteConstructors(mainWriter, type);
            WriteBody(mainWriter, constructionBuilder, bodyBuilder, type);
            mainWriter.Indent--;
            mainWriter.WriteLine("}");

            if (hasNamespace)
            {
                mainWriter.Indent--;
                mainWriter.WriteLine("}");
            }

            return builder.ToString();
        }

        private void WriteHeader(StringBuilder builder, Type type)
        {
            builder.AppendLine("// -------------------------------------------------------------------");
            builder.AppendLine("// \t\t\tAUTO-GENERATED");
            builder.AppendLine("//");
            builder.AppendLine("// \tOriginal:");
            builder.AppendLine($"// \t{type.AssemblyQualifiedName}");
            builder.AppendLine("// -------------------------------------------------------------------");
        }

        private void WriteNamespaces(StringBuilder builder, Type type)
        {
            List<string> namespaces = GetNamespaces(type);
            namespaces.Add("System");
            namespaces.Add("System.Reflection");
            namespaces.Add("TNRD.Reflectives");
            namespaces.Add("UnityEditor.PackageManager");
            namespaces = namespaces.OrderBy(x => x)
                .Distinct()
                .ToList();

            foreach (string ns in namespaces)
            {
                builder.AppendLine($"using {ns};");
            }
        }

        private void WriteConstructors(IndentedTextWriter writer, Type type)
        {
            writer.WriteLine($"public {type.Name}(object instance) : base(instance)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("Construct();");
            writer.WriteLine("Initialize();");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine($"public {type.Name}(Type type) : base(type)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("Construct();");
            writer.WriteLine("Initialize();");
            writer.Indent--;
            writer.WriteLine("}");
        }

        private void WriteBody(IndentedTextWriter writer, StringBuilder constructionBuilder, StringBuilder bodyBuilder, Type type)
        {
            writer.WriteLine("private void Construct()");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine(constructionBuilder.ToString().TrimEnd());
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine("partial void Initialize();");
            writer.WriteLine(bodyBuilder.ToString().TrimEnd());
            writer.WriteLine("public static Type GetOriginalType()");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"return System.Type.GetType(\"{type.AssemblyQualifiedName}\");");
            writer.Indent--;
            writer.WriteLine("}");
        }

        private List<string> GetNamespaces(Type type)
        {
            List<string> namespaces = new List<string>();
            foreach (FieldInfo field in type.GetFields(FLAGS))
            {
                namespaces.AddRange(GetNamespacesIncludingGenerics(field.FieldType));
            }

            foreach (PropertyInfo property in type.GetProperties(FLAGS))
            {
                namespaces.AddRange(GetNamespacesIncludingGenerics(property.PropertyType));
            }

            type.GetMethods(FLAGS);
            return namespaces;
        }

        private List<string> GetNamespacesIncludingGenerics(Type type)
        {
            List<string> namespaces = new List<string>();

            namespaces.Add(type.Namespace);

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length != 0)
            {
                namespaces.Add(type.GetGenericTypeDefinition().Namespace);
                namespaces.AddRange(genericArguments.SelectMany(GetNamespacesIncludingGenerics));
            }

            return namespaces;
        }
    }
}