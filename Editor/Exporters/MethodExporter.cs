using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;

namespace TNRD.Reflectives.Exporters
{
    public class MethodExporter : MemberExporter
    {
        public override void Export(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            ExportMethods(type, definitionWriter, constructionWriter, bodyWriter);
        }

        private void ExportMethods(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            MethodInfo[] methods = type.GetMethods(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !IsSpecialMethod(x))
                .ToArray();

            Dictionary<string, List<MethodInfo>> groups = methods.GroupBy(x => x.Name)
                .ToList()
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (MethodInfo method in methods)
            {
                List<MethodInfo> group = groups[method.Name];
                int index = group.IndexOf(method) + 1;
                ExportMethod(method, index, definitionWriter, constructionWriter, bodyWriter);
            }
        }

        private void ExportMethod(
            MethodInfo method,
            int index,
            IndentedTextWriter definitionWriter,
            IndentedTextWriter constructionWriter,
            IndentedTextWriter bodyWriter
        )
        {
            string memberName = method.Name;
            string returnTypeName = method.ReturnType.GetNiceName().Replace(".", "_");
            string paramsNamesWithType = GetMethodParametersWithType(method);
            string paramsNamesWithoutType = GetMethodParametersWithoutType(method);

            bool hasReturnType = method.ReturnType != typeof(void);
            bool isReturnTypePublic = hasReturnType && IsPublic(method.ReturnType);

            definitionWriter.WriteLine($"private ReflectiveMethod method_{memberName}_{index};");
            constructionWriter.WriteLine($"method_{memberName}_{index} = CreateMethod(\"{memberName}\", {GetBindingFlags(method)}, {GetMethodParameterTypes(method)});");

            if (hasReturnType)
            {
                bodyWriter.WriteLine($"public {returnTypeName} {memberName}({paramsNamesWithType})");
                bodyWriter.WriteLine("{");
                bodyWriter.Indent++;

                if (isReturnTypePublic)
                {
                    bodyWriter.WriteLine($"return ({returnTypeName}) method_{memberName}_{index}.Invoke({paramsNamesWithoutType});");
                }
                else
                {
                    string invokeLine = $"method_{memberName}_{index}.Invoke({paramsNamesWithoutType})";

                    if (method.ReturnType.IsEnum)
                    {
                        bodyWriter.WriteLine($"return ({returnTypeName})({method.ReturnType.GetEnumUnderlyingType().GetNiceName()}){invokeLine};");
                    }
                    else if (IsEnumerable(method.ReturnType))
                    {
                        bodyWriter.WriteLine($"return Utilities.GenerateEnumerable<{method.ReturnType.GetGenericArguments()[0].GetNiceName()}>({invokeLine});");
                    }
                    else
                    {
                        bodyWriter.WriteLine($"return new {returnTypeName}({invokeLine});");
                    }
                }

                bodyWriter.Indent--;
                bodyWriter.WriteLine("}");
            }
            else
            {
                bodyWriter.WriteLine($"public void {memberName}({paramsNamesWithType})");
                bodyWriter.WriteLine("{");
                bodyWriter.Indent++;
                bodyWriter.WriteLine($"method_{memberName}_{index}.Invoke({paramsNamesWithoutType});");
                bodyWriter.Indent--;
                bodyWriter.WriteLine("}");
            }
        }

        private bool IsSpecialMethod(MethodInfo methodInfo)
        {
            return methodInfo.Name.StartsWith("get_") ||
                   methodInfo.Name.StartsWith("set_") ||
                   methodInfo.Name.StartsWith("add_") ||
                   methodInfo.Name.StartsWith("remove_") ||
                   methodInfo.Name.StartsWith("<");
        }

        private string GetMethodParameterTypes(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            foreach (ParameterInfo parameter in parameters)
            {
                builder.Append($"typeof({parameter.ParameterType.GetNiceName()}),");
            }

            if (parameters.Length == 0)
            {
                builder.Append("null");
            }

            return builder.ToString().TrimEnd(',');
        }

        private string GetMethodParametersWithType(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            foreach (ParameterInfo parameter in parameters)
            {
                builder.Append(parameter.ParameterType.GetNiceName());
                builder.Append($" {parameter.Name},");
            }

            return builder.ToString().TrimEnd(',');
        }

        private string GetMethodParametersWithoutType(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            foreach (ParameterInfo parameter in parameters)
            {
                if (parameter.ParameterType.IsEnum)
                {
                    Type underlyingType = parameter.ParameterType.GetEnumUnderlyingType();
                    builder.Append($"({underlyingType.GetNiceName()}){parameter.Name},");
                }
                else
                {
                    builder.Append($"{parameter.Name},");
                }
            }

            return builder.ToString().TrimEnd(',');
        }
    }
}