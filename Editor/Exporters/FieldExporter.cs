using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class FieldExporter
    {
        public static CodeTypeMemberCollection Generate(Type type)
        {
            FieldExporter fieldExporter = new FieldExporter(type);
            fieldExporter.Generate();
            return fieldExporter.members;
        }

        private readonly CodeTypeMemberCollection members = new CodeTypeMemberCollection();
        private readonly Type type;

        private FieldExporter(Type type)
        {
            this.type = type;
        }

        private void Generate()
        {
            FieldInfo[] fieldInfos = type.GetFields(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.Name.Contains("<"))
                .ToArray();

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                GenerateField(fieldInfo);
                GenerateProperty(fieldInfo);
            }
        }

        private void GenerateField(FieldInfo fieldInfo)
        {
            CodeMemberField field = new CodeMemberField(typeof(ReflectiveField), $"field_{fieldInfo.Name}")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };

            members.Add(field);
        }

        private void GenerateProperty(FieldInfo fieldInfo)
        {
            CodeMemberProperty property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                HasGet = true,
                HasSet = !fieldInfo.IsInitOnly,
                Name = fieldInfo.Name,
                Type = GetCodeTypeReference(fieldInfo.FieldType)
            };

            AddPropertyGetter(property, fieldInfo);
            AddPropertySetter(property, fieldInfo);

            members.Add(property);
        }

        private static CodeTypeReference GetCodeTypeReference(Type fieldType)
        {
            if (MemberExporter.IsPublic(fieldType))
            {
                return new CodeTypeReference(fieldType);
            }

            if (MemberExporter.IsDictionary(fieldType))
            {
                Type[] genericArguments = fieldType.GetGenericArguments();
                Type first = genericArguments[0];
                Type second = genericArguments[1];

                CodeTypeReference codeTypeReference = new CodeTypeReference(typeof(Dictionary<,>));
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(first));
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(second));

                return codeTypeReference;
            }

            if (MemberExporter.IsEnumerable(fieldType))
            {
                Type[] genericArguments = fieldType.GetGenericArguments();
                Type first = genericArguments[0];

                CodeTypeReference codeTypeReference = new CodeTypeReference(typeof(IEnumerable<>));
                codeTypeReference.TypeArguments.Add(GetCodeTypeReference(first));

                return codeTypeReference;
            }

            return new CodeTypeReference(fieldType.Name);
        }

        private void AddPropertyGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            if (MemberExporter.IsPublic(fieldInfo.FieldType))
            {
                AddPublicGetter(property, fieldInfo);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                AddEnumGetter(property, fieldInfo);
            }
            else if (MemberExporter.IsDictionary(fieldInfo.FieldType))
            {
                AddDictionaryGetter(property, fieldInfo);
            }
            else if (MemberExporter.IsEnumerable(fieldInfo.FieldType))
            {
                AddEnumerableGetter(property, fieldInfo);
            }
            else
            {
                AddDefaultGetter(property, fieldInfo);
            }
        }

        private static void AddPublicGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            CodeMethodReturnStatement expression = new CodeMethodReturnStatement(
                new CodeCastExpression(
                    fieldInfo.FieldType,
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), $"field_{fieldInfo.Name}"),
                        "GetValue")));

            property.GetStatements.Add(expression);
        }

        private static void AddEnumGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            Type underlyingType = fieldInfo.FieldType.GetEnumUnderlyingType();
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                underlyingType,
                "_temp",
                new CodeCastExpression(
                    underlyingType,
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            $"field_{fieldInfo.Name}"),
                        "GetValue")));

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(
                new CodeCastExpression(
                    fieldInfo.FieldType.Name,
                    new CodeVariableReferenceExpression("_temp")));

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(returnStatement);
        }

        private void AddDictionaryGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "GetValue"));

            CodeConditionStatement conditionStatement = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("_temp"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(null)),
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(
                        new CodePrimitiveExpression(null))
                },
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(typeof(Utilities)),
                                "GenerateDictionary",
                                GetCodeTypeReference(fieldInfo.FieldType.GetGenericArguments()[0]),
                                GetCodeTypeReference(fieldInfo.FieldType.GetGenericArguments()[1])),
                            new CodeVariableReferenceExpression("_temp")))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        private void AddEnumerableGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "GetValue"));

            CodeConditionStatement conditionStatement = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("_temp"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(null)),
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(
                        new CodePrimitiveExpression(null))
                },
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(typeof(Utilities)),
                                "GenerateEnumerable",
                                GetCodeTypeReference(fieldInfo.FieldType.GetGenericArguments()[0])),
                            new CodeVariableReferenceExpression("_temp")))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        private static void AddDefaultGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "GetValue"));

            CodeConditionStatement conditionStatement = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("_temp"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(null))
                ,
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(new CodePrimitiveExpression(null))
                },
                new CodeStatement[]
                {
                    new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(
                            fieldInfo.FieldType.Name,
                            new CodeVariableReferenceExpression("_temp")))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        private void AddPropertySetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            if (!property.HasSet)
                return;

            if (MemberExporter.IsPublic(fieldInfo.FieldType))
            {
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "SetValue",
                    new CodePropertySetValueReferenceExpression());

                property.SetStatements.Add(expression);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                Type underlyingType = fieldInfo.FieldType.GetEnumUnderlyingType();
                CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                    underlyingType,
                    "_temp",
                    new CodeCastExpression(
                        underlyingType,
                        new CodePropertySetValueReferenceExpression()));

                CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "SetValue",
                    new CodeVariableReferenceExpression("_temp"));

                property.SetStatements.Add(variableDeclaration);
                property.SetStatements.Add(invokeExpression);
            }
            else if (MemberExporter.IsDictionary(fieldInfo.FieldType))
            {
                property.SetStatements.Add(
                    new CodeCommentStatement("Not supported"));
            }
            else if (MemberExporter.IsEnumerable(fieldInfo.FieldType))
            {
                property.SetStatements.Add(
                    new CodeCommentStatement("Not supported"));
            }
            else
            {
                CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"field_{fieldInfo.Name}"),
                    "SetValue",
                    new CodeFieldReferenceExpression(
                        new CodePropertySetValueReferenceExpression(),
                        "Instance"));

                property.SetStatements.Add(invokeExpression);
            }
        }
    }
}
