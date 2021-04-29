using System;
using System.CodeDom;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class ExportFunctions
    {
        private static string GetTag(MemberTypes memberTypes)
        {
            switch (memberTypes)
            {
                case MemberTypes.All:
                case MemberTypes.Constructor:
                case MemberTypes.Custom:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Method:
                case MemberTypes.NestedType:
                    throw new NotSupportedException();
                case MemberTypes.Field:
                    return "field";
                case MemberTypes.Property:
                    return "property";
            }

            throw new NotImplementedException();
        }

        internal static void AddPublicGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeMethodReturnStatement expression = new CodeMethodReturnStatement(
                new CodeCastExpression(
                    type,
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), $"{GetTag(memberTypes)}_{name}"),
                        "GetValue")));

            property.GetStatements.Add(expression);
        }

        internal static void AddEnumGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            Type underlyingType = type.GetEnumUnderlyingType();
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                underlyingType,
                "_temp",
                new CodeCastExpression(
                    underlyingType,
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            $"{GetTag(memberTypes)}_{name}"),
                        "GetValue")));

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(
                new CodeCastExpression(
                    type.Name,
                    new CodeVariableReferenceExpression("_temp")));

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(returnStatement);
        }

        internal static void AddDictionaryGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"{GetTag(memberTypes)}_{name}"),
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
                        new CodeObjectCreateExpression(
                            ExportUtils.GetCodeTypeReference(type),
                            new CodeMethodInvokeExpression(
                                new CodeMethodReferenceExpression(
                                    new CodeTypeReferenceExpression(typeof(Utilities)),
                                    "GenerateDictionary",
                                    ExportUtils.GetCodeTypeReference(type.GetGenericArguments()[0]),
                                    ExportUtils.GetCodeTypeReference(type.GetGenericArguments()[1])),
                                new CodeVariableReferenceExpression("_temp"))))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        internal static void AddEnumerableGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"{GetTag(memberTypes)}_{name}"),
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
                                ExportUtils.GetCodeTypeReference(type.GetGenericArguments()[0])),
                            new CodeVariableReferenceExpression("_temp")))
                    // new CodeMethodReturnStatement(
                    //     new CodeObjectCreateExpression(
                    //         ExportUtils.GetCodeTypeReference(type),
                    //         new CodeMethodInvokeExpression(
                    //             new CodeMethodReferenceExpression(
                    //                 new CodeTypeReferenceExpression(typeof(Utilities)),
                    //                 "GenerateEnumerable",
                    //                 ExportUtils.GetCodeTypeReference(type.GetGenericArguments()[0])),
                    //             new CodeVariableReferenceExpression("_temp"))))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        internal static void AddArrayGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"{GetTag(memberTypes)}_{name}"),
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
                                "GenerateArray",
                                ExportUtils.GetCodeTypeReference(type.GetElementType())),
                            new CodeVariableReferenceExpression("_temp")))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        internal static void AddDefaultGetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                typeof(object),
                "_temp",
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        $"{GetTag(memberTypes)}_{name}"),
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
                            ExportUtils.GetCodeTypeReference(type),
                            new CodeVariableReferenceExpression("_temp")))
                });

            property.GetStatements.Add(variableDeclaration);
            property.GetStatements.Add(conditionStatement);
        }

        internal static void AddPublicSetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    $"{GetTag(memberTypes)}_{name}"),
                "SetValue",
                new CodePropertySetValueReferenceExpression());

            property.SetStatements.Add(expression);
        }

        internal static void AddEnumSetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            Type underlyingType = type.GetEnumUnderlyingType();
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement(
                underlyingType,
                "_temp",
                new CodeCastExpression(
                    underlyingType,
                    new CodePropertySetValueReferenceExpression()));

            CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    $"{GetTag(memberTypes)}_{name}"),
                "SetValue",
                new CodeVariableReferenceExpression("_temp"));

            property.SetStatements.Add(variableDeclaration);
            property.SetStatements.Add(invokeExpression);
        }

        internal static void AddDictionarySetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            AddUnsupportedSetter(property);
        }

        internal static void AddEnumerableSetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            AddUnsupportedSetter(property);
        }

        internal static void AddArraySetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            AddUnsupportedSetter(property);
        }

        private static void AddUnsupportedSetter(CodeMemberProperty property)
        {
            // TODO: Log that this is not supported instead of just this comment
            property.SetStatements.Add(
                new CodeCommentStatement("Not supported"));
        }

        internal static void AddDefaultSetter(CodeMemberProperty property, Type type, string name, MemberTypes memberTypes)
        {
            CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    $"{GetTag(memberTypes)}_{name}"),
                "SetValue",
                new CodeFieldReferenceExpression(
                    new CodePropertySetValueReferenceExpression(),
                    "Instance"));

            property.SetStatements.Add(invokeExpression);
        }
    }
}
