using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class MethodExporter
    {
        public static CodeTypeMemberCollection Generate(Type type)
        {
            MethodExporter exporter = new MethodExporter(type);
            exporter.Generate();
            return exporter.members;
        }

        public static void Iterate(Type type, Action<MethodExporter, MethodInfo> action)
        {
            MethodExporter exporter = new MethodExporter(type);
            exporter.Iterate(action);
        }

        private readonly CodeTypeMemberCollection members = new CodeTypeMemberCollection();
        private readonly Type type;
        private Dictionary<string, List<MethodInfo>> groups;

        private MethodExporter(Type type)
        {
            this.type = type;
        }

        private void Iterate(Action<MethodExporter, MethodInfo> action)
        {
            MethodInfo[] methodInfos = type.GetMethods(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !IsSpecialMethod(x))
                .Where(x => !HasSpecialParameters(x))
                .ToArray();

            groups = methodInfos.GroupBy(x => x.Name)
                .ToList()
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (MethodInfo methodInfo in methodInfos)
            {
                action.Invoke(this, methodInfo);
            }
        }

        private void Generate()
        {
            void Action(MethodExporter exporter, MethodInfo methodInfo)
            {
                GenerateField(methodInfo);
                GenerateMethod(methodInfo);
            }

            Iterate(Action);
        }

        private bool IsSpecialMethod(MethodInfo methodInfo)
        {
            return methodInfo.Name.StartsWith("get_") ||
                methodInfo.Name.StartsWith("set_") ||
                methodInfo.Name.StartsWith("add_") ||
                methodInfo.Name.StartsWith("remove_") ||
                methodInfo.Name.StartsWith("<");
        }

        private bool HasSpecialParameters(MethodInfo methodInfo)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            return parameterInfos.Any(x => x.IsOut ||
                x.ParameterType.IsByRef);
        }

        private int GetIndex(MethodInfo methodInfo)
        {
            List<MethodInfo> group = groups[methodInfo.Name];
            int index = group.IndexOf(methodInfo) + 1;
            return index;
        }

        internal string GetMethodName(MethodInfo methodInfo)
        {
            return $"method_{methodInfo.Name}_{GetIndex(methodInfo)}";
        }

        private void GenerateField(MethodInfo methodInfo)
        {
            CodeMemberField field = new CodeMemberField(typeof(ReflectiveMethod), GetMethodName(methodInfo))
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };

            members.Add(field);
        }

        private void GenerateMethod(MethodInfo methodInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = methodInfo.Name,
                ReturnType = ExportUtils.GetCodeTypeReference(methodInfo.ReturnType)
            };

            ParameterInfo[] parameters = methodInfo.GetParameters();
            AddParameters(parameters, method);

            CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    GetMethodName(methodInfo)),
                "Invoke");

            AddInvokeParameters(parameters, invokeExpression, method);

            if (methodInfo.ReturnType == typeof(void))
            {
                method.Statements.Add(invokeExpression);
            }
            else if (ExportUtils.IsPublic(methodInfo.ReturnType))
            {
                method.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeCastExpression(
                            methodInfo.ReturnType,
                            invokeExpression)));
            }
            else if (methodInfo.ReturnType.IsEnum)
            {
                AddEnumBody(methodInfo, method, invokeExpression);
            }
            else if (ExportUtils.IsDictionary(methodInfo.ReturnType))
            {
                AddDictionaryBody(methodInfo, method, invokeExpression);
            }
            else if (ExportUtils.IsEnumerable(methodInfo.ReturnType))
            {
                AddEnumerableBody(methodInfo, method, invokeExpression);
            }
            else
            {
                method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        typeof(object),
                        "temp_data",
                        invokeExpression));

                method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                        "temp_retVal",
                        new CodeObjectCreateExpression(
                            ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                            new CodeVariableReferenceExpression("temp_data"))));

                method.Statements.Add(new CodeMethodReturnStatement(
                    new CodeVariableReferenceExpression("temp_retVal")));
            }

            members.Add(method);
        }

        private static void AddParameters(ParameterInfo[] parameters, CodeMemberMethod method)
        {
            foreach (ParameterInfo parameterInfo in parameters)
            {
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(
                    ExportUtils.GetCodeTypeReference(parameterInfo.ParameterType), parameterInfo.Name);

                if (parameterInfo.IsIn)
                {
                    expression.Direction = FieldDirection.In;
                }
                else if (parameterInfo.ParameterType.IsByRef)
                {
                    expression.Direction = FieldDirection.Ref;
                }
                else if (parameterInfo.IsOut)
                {
                    expression.Direction = FieldDirection.Out;
                }

                method.Parameters.Add(expression);
            }
        }

        private static void AddInvokeParameters(ParameterInfo[] parameters, CodeMethodInvokeExpression invokeExpression, CodeMemberMethod method)
        {
            foreach (ParameterInfo parameterInfo in parameters)
            {
                if (ExportUtils.IsPublic(parameterInfo.ParameterType))
                {
                    invokeExpression.Parameters.Add(
                        new CodeArgumentReferenceExpression(parameterInfo.Name));
                }
                else
                {
                    AddInvokePrivateParameters(invokeExpression, method, parameterInfo);
                }
            }
        }

        private static void AddInvokePrivateParameters(CodeMethodInvokeExpression invokeExpression, CodeMemberMethod method, ParameterInfo parameterInfo)
        {
            if (ExportUtils.IsDictionary(parameterInfo.ParameterType))
            {
                Type[] genericArguments = parameterInfo.ParameterType.GetGenericArguments();

                method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        typeof(IEnumerable), $"temp_{parameterInfo.Name}",
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(Utilities)),
                            "ConvertDictionaryParameter",
                            new CodeArgumentReferenceExpression(parameterInfo.Name),
                            new CodePrimitiveExpression(genericArguments[0].AssemblyQualifiedName),
                            new CodePrimitiveExpression(genericArguments[1].AssemblyQualifiedName))));

                invokeExpression.Parameters.Add(
                    new CodeVariableReferenceExpression($"temp_{parameterInfo.Name}"));
            }
            else if (ExportUtils.IsEnumerable(parameterInfo.ParameterType))
            {
                Type[] genericArguments = parameterInfo.ParameterType.GetGenericArguments();

                method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        typeof(IEnumerable), $"temp_{parameterInfo.Name}",
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(Utilities)),
                            "ConvertEnumerableParameter",
                            new CodeArgumentReferenceExpression(parameterInfo.Name),
                            new CodePrimitiveExpression(genericArguments[0].AssemblyQualifiedName))));

                invokeExpression.Parameters.Add(
                    new CodeVariableReferenceExpression($"temp_{parameterInfo.Name}"));
            }
            else if (parameterInfo.ParameterType.IsEnum)
            {
                invokeExpression.Parameters.Add(
                    new CodeCastExpression(
                        parameterInfo.ParameterType.GetEnumUnderlyingType(),
                        new CodeArgumentReferenceExpression(
                            parameterInfo.Name)));
            }
            else
            {
                invokeExpression.Parameters.Add(
                    new CodeArgumentReferenceExpression(
                        $"{parameterInfo.Name}.Instance"));
            }
        }

        private static void AddEnumBody(MethodInfo methodInfo, CodeMemberMethod method, CodeMethodInvokeExpression invokeExpression)
        {
            method.Statements.Add(
                new CodeVariableDeclarationStatement(
                    methodInfo.ReturnType.GetEnumUnderlyingType(),
                    "temp_retVal",
                    new CodeCastExpression(methodInfo.ReturnType.GetEnumUnderlyingType(),
                        invokeExpression)));

            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeCastExpression(
                    methodInfo.ReturnType.Name,
                    new CodeVariableReferenceExpression("temp_retVal"))));
        }

        private static void AddDictionaryBody(MethodInfo methodInfo, CodeMemberMethod method, CodeMethodInvokeExpression invokeExpression)
        {
            method.Statements.Add(
                new CodeVariableDeclarationStatement(
                    typeof(object),
                    "temp_data",
                    invokeExpression));

            CodeExpression invokeGenerateExpression = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        typeof(Utilities)),
                    "GenerateDictionary",
                    ExportUtils.GetCodeTypeReference(methodInfo.ReturnType.GetGenericArguments()[0]),
                    ExportUtils.GetCodeTypeReference(methodInfo.ReturnType.GetGenericArguments()[1])),
                new CodeVariableReferenceExpression("temp_data"));

            method.Statements.Add(
                new CodeVariableDeclarationStatement(
                    ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                    "temp_retVal",
                    methodInfo.ReturnType.IsInterface
                        ? invokeGenerateExpression
                        : new CodeObjectCreateExpression(
                            ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                            invokeGenerateExpression)));

            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeVariableReferenceExpression("temp_retVal")));
        }

        private static void AddEnumerableBody(MethodInfo methodInfo, CodeMemberMethod method, CodeMethodInvokeExpression invokeExpression)
        {
            method.Statements.Add(
                new CodeVariableDeclarationStatement(
                    typeof(object),
                    "temp_data",
                    invokeExpression));

            CodeExpression invokeGenerateExpression = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        typeof(Utilities)),
                    "GenerateEnumerable",
                    ExportUtils.GetCodeTypeReference(methodInfo.ReturnType.GetGenericArguments()[0])),
                new CodeVariableReferenceExpression("temp_data"));

            method.Statements.Add(
                new CodeVariableDeclarationStatement(
                    ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                    "temp_retVal",
                    methodInfo.ReturnType.IsInterface
                        ? invokeGenerateExpression
                        : new CodeObjectCreateExpression(
                            ExportUtils.GetCodeTypeReference(methodInfo.ReturnType),
                            invokeGenerateExpression)));

            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeVariableReferenceExpression("temp_retVal")));
        }
    }
}
