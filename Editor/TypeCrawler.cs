using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives
{
    public class TypeCrawler
    {
        public static List<Type> GetInferredTypes(Type type)
        {
            TypeCrawler typeCrawler = new TypeCrawler(type);
            typeCrawler.Crawl();
            return typeCrawler.inferredTypes
                .Where(IsValid)
                .ToList();
        }

        private static bool IsValid(Type type)
        {
            if (type == null)
                return false;

            if (string.IsNullOrEmpty(type.Namespace))
                return true;

            if (type.Namespace.StartsWith("System"))
                return false;
            if (type.Namespace.StartsWith("Mono"))
                return false;

            return true;
        }

        private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Type rootType;

        private readonly HashSet<Type> inferredTypes = new HashSet<Type>();
        private readonly HashSet<Type> alreadyChecked = new HashSet<Type>();

        private List<Type> typesToCrawl = new List<Type>();

        private TypeCrawler(Type rootType)
        {
            this.rootType = rootType;
        }

        private void Crawl()
        {
            typesToCrawl.Add(rootType);

            while (typesToCrawl.Count > 0)
            {
                Type type = typesToCrawl[0];
                typesToCrawl.RemoveAt(0);
                Crawl(type);
            }
        }

        private void Crawl(Type type)
        {
            if (alreadyChecked.Contains(type))
                return;

            alreadyChecked.Add(type);
            inferredTypes.Add(type);

            CrawFields(type);
            CrawlProperties(type);
            CrawlMethods(type);
        }

        private void CrawFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(FLAGS);

            foreach (FieldInfo field in fields)
            {
                CrawlMemberType(field.FieldType);
            }
        }

        private void CrawlProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(FLAGS);

            foreach (PropertyInfo property in properties)
            {
                CrawlMemberType(property.PropertyType);
            }
        }

        private void CrawlMethods(Type type)
        {
            MethodInfo[] methods = type.GetMethods(FLAGS);

            foreach (MethodInfo method in methods)
            {
                Type returnType = method.ReturnType;
                if (returnType != typeof(void))
                {
                    CrawlMemberType(returnType);
                }

                ParameterInfo[] parameters = method.GetParameters();

                foreach (ParameterInfo parameter in parameters)
                {
                    CrawlMemberType(parameter.ParameterType);
                }
            }
        }

        private void CrawlMemberType(Type memberType)
        {
            if (alreadyChecked.Contains(memberType))
                return;

            Type[] genericArguments = memberType.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                typesToCrawl.Add(memberType);
                return;
            }

            // Type genericTypeDefinition = memberType.GetGenericTypeDefinition();
            // types.Add(genericTypeDefinition);

            foreach (Type genericArgument in genericArguments)
            {
                if (!alreadyChecked.Contains(genericArgument))
                    typesToCrawl.Add(genericArgument);
            }
        }
    }
}