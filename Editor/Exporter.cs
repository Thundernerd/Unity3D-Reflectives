using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

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
            string output;

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = false
            };

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(@namespace);
            codeNamespace.Types.Add(Exporters.Exporter.Export(type));
            compileUnit.Namespaces.Add(codeNamespace);

            using (StringWriter writer = new StringWriter())
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                output = writer.ToString();
            }

            string sanitizedFilename = $"{type.GetNiceName().Replace(".", "_")}.Reflected.cs";
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
    }
}
