using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives
{
    internal class TypeCrawler
    {
        public static List<Type> GetInferredTypes(Type type)
        {
            return new TypeCrawler(type)
                .Crawl();
        }

        private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Type rootType;

        private readonly HashSet<Type> typesChecked = new HashSet<Type>();

        private TypeCrawler(Type type)
        {
            rootType = type;
        }

        private List<Type> Crawl()
        {
            Crawl(rootType);
            return typesChecked.ToList();
        }

        private void Crawl(Type type)
        {
            if (type.Namespace?.StartsWith("System") ?? false)
                return;
            if (type.Namespace?.StartsWith("Mono") ?? false)
                return;
            if (type.IsPublic)
                return;

            typesChecked.Add(type);

            CrawlFields(type);
            CrawlProperties(type);
            CrawlMethods(type);
        }

        private void CrawlFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(FLAGS);
            foreach (FieldInfo field in fields)
            {
                if (field.DeclaringType != type)
                    continue;

                Type memberType = field.FieldType;
                CheckType(memberType);
            }
        }

        private void CrawlProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(FLAGS);
            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType != type)
                    continue;

                Type memberType = property.PropertyType;
                CheckType(memberType);
            }
        }

        private void CrawlMethods(Type type)
        {
            MethodInfo[] methods = type.GetMethods(FLAGS);
            foreach (MethodInfo method in methods)
            {
                if (method.DeclaringType != type)
                    continue;

                Type returnType = method.ReturnType;
                if (returnType != typeof(void))
                {
                    CheckType(type);
                }

                ParameterInfo[] parameters = method.GetParameters();
                foreach (ParameterInfo parameter in parameters)
                {
                    CheckType(parameter.ParameterType);
                }
            }
        }

        private void CheckType(Type type)
        {
            if (typesChecked.Contains(type))
                return;

            Crawl(type);

            if (type.GenericTypeArguments.Length == 0)
                return;

            foreach (Type genericType in type.GenericTypeArguments)
            {
                if (typesChecked.Contains(genericType))
                    continue;

                Crawl(genericType);
            }
        }
    }
}
