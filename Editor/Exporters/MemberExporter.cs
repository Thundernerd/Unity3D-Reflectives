using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace TNRD.Reflectives.Exporters
{
    internal abstract class MemberExporter
    {
        public abstract void Export(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter);

        protected string GetBindingFlags(MemberInfo memberInfo)
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

        protected bool IsStatic(MemberInfo memberInfo)
        {
            if (memberInfo is EventInfo eventInfo)
            {
                return eventInfo.GetAddMethod().IsStatic;
            }

            return memberInfo.IsStatic();
        }

        protected bool IsPublic(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.IsPublic;
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetGetMethod()?.IsPublic ?? false;
                case MethodInfo methodInfo:
                    return methodInfo.IsPublic;
                case EventInfo eventInfo:
                    return eventInfo.GetAddMethod().IsPublic;
            }

            return false;
        }

        protected static bool IsPublic(Type type)
        {
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 0)
                return type.IsPublic;

            return type.IsPublic && genericArguments.All(IsPublic);
        }

        protected static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        protected static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }
    }
}