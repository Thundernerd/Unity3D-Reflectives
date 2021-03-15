using System;
using System.CodeDom;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class ClassExporter
    {
        public static CodeTypeDeclaration Generate(Type type)
        {
            return new ClassExporter(type).Generate();
        }

        private readonly Type type;

        private ClassExporter(Type type)
        {
            this.type = type;
        }

        private CodeTypeDeclaration Generate()
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(type.Name)
            {
                IsClass = true,
                IsPartial = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            declaration.Comments.Add(new CodeCommentStatement($"Original: {type.AssemblyQualifiedName}"));
            declaration.BaseTypes.Add(new CodeTypeReference(typeof(ReflectiveClass)));

            AddConstructors(declaration);
            AddMethods(declaration);

            CodeTypeMemberCollection codeTypeMembers = FieldExporter.Generate(type);
            declaration.Members.AddRange(codeTypeMembers);

            return declaration;
        }

        private void AddConstructors(CodeTypeDeclaration declaration)
        {
            AddInstanceConstructor(declaration);
            AddTypeConstructor(declaration);
        }

        private void AddInstanceConstructor(CodeTypeDeclaration declaration)
        {
            CodeConstructor constructor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };

            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "instance"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("instance"));

            AddConstructorMethods(constructor);

            declaration.Members.Add(constructor);
        }

        private void AddTypeConstructor(CodeTypeDeclaration declaration)
        {
            CodeConstructor constructor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };

            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Type), "type"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("type"));

            AddConstructorMethods(constructor);

            declaration.Members.Add(constructor);
        }

        private void AddConstructorMethods(CodeConstructor constructor)
        {
            CodeMethodInvokeExpression invokeConstructExpression = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(), "Construct");
            CodeMethodInvokeExpression invokeInitializeExpression = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(), "Initialize");

            constructor.Statements.Add(invokeConstructExpression);
            constructor.Statements.Add(invokeInitializeExpression);
        }

        private void AddMethods(CodeTypeDeclaration declaration)
        {
            AddConstructMethod(declaration);
            AddInitializeMethod(declaration);
            AddGetOriginalTypeMethod(declaration);
        }

        private void AddConstructMethod(CodeTypeDeclaration declaration)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "Construct"
            };

            declaration.Members.Add(method);
        }

        private void AddInitializeMethod(CodeTypeDeclaration declaration)
        {
            // Hack to create a method without a body
            CodeMemberField field = new CodeMemberField
            {
                Attributes = MemberAttributes.ScopeMask,
                Name = "Initialize()",
                Type = new CodeTypeReference("partial void")
            };

            declaration.Members.Add(field);
        }

        private void AddGetOriginalTypeMethod(CodeTypeDeclaration declaration)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static,
                Name = "GetOriginalType",
                ReturnType = new CodeTypeReference(typeof(Type))
            };

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement();
            CodeMethodInvokeExpression invokeExpression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(Type)),
                "GetType",
                new CodePrimitiveExpression(type.AssemblyQualifiedName));
            returnStatement.Expression = invokeExpression;
            method.Statements.Add(returnStatement);

            declaration.Members.Add(method);
        }
    }
}
