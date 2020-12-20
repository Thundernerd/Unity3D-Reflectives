using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace TNRD.Reflectives
{
    public class ReflectiveGeneratorWindow : OdinEditorWindow
    {
        [MenuItem("TNRD/Reflective Generator")]
        private static void Open()
        {
            ReflectiveGeneratorWindow wnd = GetWindow<ReflectiveGeneratorWindow>("Reflective Generator");
            wnd.Show();
        }

        private Type[] types;
        private ValueDropdownList<int> typeItems;
        private Type[] baseTypes;
        private ValueDropdownList<int> baseTypeItems;

        [SerializeField]
        [ValueDropdown(nameof(GetTypes))]
        [OnValueChanged(nameof(OnSelectedTypeChanged))]
        private int selectedType;

        [SerializeField]
        [ValueDropdown(nameof(GetBaseTypes))]
        private int selectedBaseType;

        [SerializeField]
        private string @namespace;

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [HideLabel]
        private TypeDrawer typeDrawer;

        [OnInspectorInit]
        private void OnInitialize()
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetTypes().Any())
                .OrderBy(x => x.GetName().FullName)
                .SelectMany(x => x.GetTypes().OrderBy(y => y.FullName))
                .Where(x => !x.IsSpecialName && !x.Name.StartsWith("<"))
                .ToArray();

            typeItems = new ValueDropdownList<int>();

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                typeItems.Add(new ValueDropdownItem<int>(type.GetNiceFullName().Replace('.', '/'), i));
            }

            baseTypes = types.Where(x => x.ImplementsOrInherits(typeof(ReflectiveClass)))
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsSealed)
                .ToArray();

            baseTypeItems = new ValueDropdownList<int>();

            for (int i = 0; i < baseTypes.Length; i++)
            {
                Type baseType = baseTypes[i];
                baseTypeItems.Add(new ValueDropdownItem<int>(baseType.GetNiceFullName().Replace('.', '/'), i));
            }

            typeDrawer = CreateInstance<TypeDrawer>();
            typeDrawer.Load(types[selectedType]);
        }

        [OnInspectorDispose]
        private void Dispose()
        {
            if (typeDrawer != null)
            {
                DestroyImmediate(typeDrawer);
            }
        }

        private IEnumerable GetTypes()
        {
            return typeItems;
        }

        private IEnumerable GetBaseTypes()
        {
            return baseTypeItems;
        }

        private void OnSelectedTypeChanged()
        {
            if (typeDrawer == null)
            {
                typeDrawer = CreateInstance<TypeDrawer>();
            }

            typeDrawer.Load(types[selectedType]);
        }

        [Button(ButtonSizes.Gigantic)]
        private void Export()
        {
            Type type = types[selectedType];
            StringBuilder builder = new StringBuilder();

            StringBuilder definitionBuilder = new StringBuilder();
            StringBuilder constructorBuilder = new StringBuilder();
            StringBuilder bodyBuilder = new StringBuilder();
            List<string> namespaces = new List<string>()
            {
                "System.Reflection",
                "TNRD.Reflectives"
            };

            ExportFields(definitionBuilder, constructorBuilder, bodyBuilder, namespaces);
            ExportProperties(definitionBuilder, constructorBuilder, bodyBuilder, namespaces);
            ExportMethods(definitionBuilder, constructorBuilder, bodyBuilder, namespaces);

            namespaces = namespaces.Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (string ns in namespaces)
            {
                builder.AppendLine($"using {ns};");
            }

            builder.AppendLine($"public sealed class {type.GetCompilableNiceName()} : {baseTypes[selectedBaseType].GetCompilableNiceFullName()}");
            builder.AppendLine("{");

            builder.AppendLine("#region Definitions");
            builder.AppendLine(definitionBuilder.ToString().TrimEnd());
            builder.AppendLine("#endregion");

            builder.AppendLine($"\tpublic {type.GetCompilableNiceName()}(object instance) : base(instance)");
            builder.AppendLine("\t{");

            builder.AppendLine(constructorBuilder.ToString().TrimEnd());

            builder.AppendLine("\t}");

            builder.AppendLine(bodyBuilder.ToString().TrimEnd());

            builder.Append("}");

            string path = EditorUtility.SaveFilePanelInProject("Save Reflective Class", type.GetCompilableNiceName(), "cs", string.Empty);

            string content = builder.ToString();

            if (!string.IsNullOrEmpty(@namespace))
            {
                string[] lines = content.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                StringBuilder namespaceBuilder = new StringBuilder();
                namespaceBuilder.AppendLine($"namespace {@namespace}");
                namespaceBuilder.AppendLine("{");
                foreach (string line in lines)
                {
                    namespaceBuilder.AppendLine($"\t{line}");
                }

                namespaceBuilder.AppendLine("}");
                content = namespaceBuilder.ToString();
            }

            File.WriteAllText(path, content);

            AssetDatabase.Refresh();
        }

        private void ExportFields(StringBuilder definitionBuilder, StringBuilder constructorBuilder, StringBuilder bodyBuilder, List<string> namespaces)
        {
            bodyBuilder.AppendLine("#region Fields");

            foreach (FieldInfo field in typeDrawer.Fields)
            {
                namespaces.Add(field.FieldType.Namespace);

                string typeName = field.FieldType.GetNiceName();
                string fieldName = field.Name;

                ExportFieldDefinition(field, definitionBuilder, typeName, fieldName);
                ExportFieldConstruction(field, constructorBuilder, typeName, fieldName);
                ExportFieldBody(field, bodyBuilder, typeName, fieldName);
            }

            bodyBuilder.AppendLine("#endregion");
        }

        private void ExportFieldDefinition(FieldInfo field, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(field.FieldType))
            {
                builder.AppendLine($"\tprivate ReflectiveField<{typeName}> field_{memberName};");
            }
            else
            {
                builder.AppendLine($"\tprivate ReflectiveField field_{memberName};");
            }
        }

        private void ExportFieldConstruction(FieldInfo field, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(field.FieldType))
            {
                builder.AppendLine($"\t\tfield_{memberName} = CreateField<{typeName}>(\"{memberName}\", {GetBindingFlags(field)});");
            }
            else
            {
                builder.AppendLine($"\t\tfield_{memberName} = CreateField(\"{memberName}\", {GetBindingFlags(field)});");
            }
        }

        private void ExportFieldBody(FieldInfo field, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(field.FieldType))
            {
                builder.AppendLine($"\tpublic {typeName} {memberName}");
            }
            else
            {
                builder.AppendLine("\t/// <summary>");
                builder.AppendLine($"\t/// Return type: {typeName}");
                builder.AppendLine("\t/// </summary>");
                builder.AppendLine($"\tpublic object {memberName}");
            }

            builder.AppendLine("\t{");
            builder.AppendLine($"\t\tget => field_{memberName}.GetValue();");
            builder.AppendLine($"\t\tset => field_{memberName}.SetValue(value);");
            builder.AppendLine("\t}");
        }

        private void ExportProperties(StringBuilder definitionBuilder, StringBuilder constructorBuilder, StringBuilder bodyBuilder, List<string> namespaces)
        {
            bodyBuilder.AppendLine("#region Properties");

            foreach (PropertyInfo property in typeDrawer.Properties)
            {
                namespaces.Add(property.PropertyType.Namespace);

                string typeName = property.PropertyType.GetNiceName();
                string memberName = property.Name;

                ExportPropertyDefinition(property, definitionBuilder, typeName, memberName);
                ExportPropertyConstruction(property, constructorBuilder, typeName, memberName);
                ExportPropertyBody(property, bodyBuilder, typeName, memberName);
            }

            bodyBuilder.AppendLine("#endregion");
        }

        private void ExportPropertyDefinition(PropertyInfo property, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(property.PropertyType))
            {
                builder.AppendLine($"\tprivate ReflectiveProperty<{typeName}> property_{memberName};");
            }
            else
            {
                builder.AppendLine($"\tprivate ReflectiveProperty property_{memberName};");
            }
        }

        private void ExportPropertyConstruction(PropertyInfo property, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(property.PropertyType))
            {
                builder.AppendLine($"\t\tproperty_{memberName} = CreateProperty<{typeName}>(\"{memberName}\", {GetBindingFlags(property)});");
            }
            else
            {
                builder.AppendLine($"\t\tproperty_{memberName} = CreateProperty(\"{memberName}\", {GetBindingFlags(property)});");
            }
        }

        private void ExportPropertyBody(PropertyInfo property, StringBuilder builder, string typeName, string memberName)
        {
            if (CanSpecifyType(property.PropertyType))
            {
                builder.AppendLine($"\tpublic {typeName} {memberName}");
            }
            else
            {
                builder.AppendLine("\t/// <summary>");
                builder.AppendLine($"\t/// Return type: {typeName}");
                builder.AppendLine("\t/// </summary>");
                builder.AppendLine($"\tpublic object {memberName}");
            }

            builder.AppendLine("\t{");

            if (property.CanRead)
            {
                builder.AppendLine($"\t\tget => property_{memberName}.GetValue();");
            }

            if (property.CanWrite)
            {
                builder.AppendLine($"\t\tset => property_{memberName}.SetValue(value);");
            }

            builder.AppendLine("\t}");
        }

        private void ExportMethods(StringBuilder definitionBuilder, StringBuilder constructorBuilder, StringBuilder bodyBuilder, List<string> namespaces)
        {
            bodyBuilder.AppendLine("#region Methods");

            foreach (MethodInfo method in typeDrawer.Methods)
            {
                namespaces.Add(method.ReturnType.Namespace);
                namespaces.AddRange(method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType.Namespace));

                string memberName = method.Name;

                definitionBuilder.AppendLine($"\tprivate ReflectiveMethod method_{memberName};");
                constructorBuilder.AppendLine($"\t\tmethod_{memberName} = CreateMethod(\"{memberName}\", {GetBindingFlags(method)});");

                bool returnsValue = method.ReturnType != typeof(void);

                bodyBuilder.AppendLine("\t/// <summary>");
                if (returnsValue)
                {
                    bodyBuilder.AppendLine($"\t/// Return type: {method.ReturnType.GetNiceName()}");
                }

                bodyBuilder.AppendLine("\t/// </summary>");
                string methodParametersSummary = GetMethodParametersSummary(method);
                if (!string.IsNullOrEmpty(methodParametersSummary))
                {
                    bodyBuilder.AppendLine(methodParametersSummary);
                }

                string returnType = returnsValue ? "object" : "void";
                bodyBuilder.AppendLine($"\tpublic {returnType} {memberName}({GetMethodParameters(method)})");
                bodyBuilder.AppendLine("\t{");
                string bodyPrefix = returnsValue ? "\t\treturn" : "\t\t";
                bodyBuilder.AppendLine($"{bodyPrefix} method_{memberName}.Invoke({string.Join(",", method.GetParameters().Select(x => x.Name))});");
                bodyBuilder.AppendLine("\t}");
            }

            bodyBuilder.AppendLine("#endregion");
        }

        private string GetMethodParametersSummary(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();

            ParameterInfo[] parameters = method.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                if (CanSpecifyType(parameter.ParameterType))
                    continue;
                builder.AppendLine($"\t/// <param name=\"{parameter.Name}\">Type: {parameter.ParameterType.GetCompilableNiceName()}</param>");
            }

            return builder.ToString().TrimEnd();
        }

        private string GetMethodParameters(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            foreach (ParameterInfo parameter in parameters)
            {
                builder.Append(CanSpecifyType(parameter.ParameterType) ? parameter.ParameterType.GetCompilableNiceName() : "object");
                builder.Append($" {parameter.Name},");
            }

            return builder.ToString().TrimEnd(',');
        }

        private string GetBindingFlags(MemberInfo memberInfo)
        {
            List<string> flags = new List<string>();

            if (IsStatic(memberInfo))
            {
                flags.Add("BindingFlags.Static");
            }
            else
            {
                flags.Add("BindingFlags.Instance");
            }

            if (IsPublic(memberInfo))
            {
                flags.Add("BindingFlags.Public");
            }
            else
            {
                flags.Add("BindingFlags.NonPublic");
            }

            return string.Join(" | ", flags);
        }

        private bool IsStatic(MemberInfo memberInfo)
        {
            return memberInfo.IsStatic();
        }

        private bool IsPublic(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.IsPublic;
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetGetMethod()?.IsPublic ?? false;
                case MethodInfo methodInfo:
                    return methodInfo.IsPublic;
            }

            return false;
        }

        private bool CanSpecifyType(Type type)
        {
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length > 0)
            {
                return type.IsPublic && genericArguments.All(CanSpecifyType);
            }
            else
            {
                return type.IsPublic;
            }
        }
    }
}