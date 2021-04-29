using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("TNRD.Reflectives.Editor")]

namespace TNRD.Reflectives
{
    /// <summary>
    /// A couple of utilities that can be used with Reflectives
    /// </summary>
    [PublicAPI]
    public static class Utilities
    {
        public static IDictionary ConvertDictionaryParameter(IDictionary dictionary, string keyType, string valueType)
        {
            Type sourceKeyType = dictionary.GetType().GetGenericArguments()[0];
            Type sourceValueType = dictionary.GetType().GetGenericArguments()[1];

            Type targetKeyType = Type.GetType(keyType);
            Type targetValueType = Type.GetType(valueType);

            IDictionary dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(targetKeyType, targetValueType));

            foreach (DictionaryEntry entry in dictionary)
            {
                object convertedKey = ConvertParameter(entry.Key, sourceKeyType, targetKeyType);
                object convertedValue = ConvertParameter(entry.Value, sourceValueType, targetValueType);
                dict.Add(convertedKey, convertedValue);
            }

            return dict;
        }

        public static IEnumerable ConvertEnumerableParameter(IEnumerable enumerable, string assemblyQualifiedTypeDefinition)
        {
            Type sourceType = enumerable.GetType().GetGenericArguments()[0];
            Type targetType = Type.GetType(assemblyQualifiedTypeDefinition);

            IList list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));

            foreach (object o in enumerable)
            {
                object converted = ConvertParameter(o, sourceType, targetType);
                list.Add(converted);
            }

            return list;
        }

        private static object ConvertParameter(object value, Type from, Type to)
        {
            if (IsPublic(to))
                return value;

            if (to.IsEnum)
            {
                Type underlyingType = to.GetEnumUnderlyingType();
                object temp = Convert.ChangeType(value, underlyingType);
                object converted = Convert.ChangeType(temp, to);
                return converted;
            }

            if (ImplementsOrInherits(from, typeof(ReflectiveClass)))
            {
                ReflectiveClass reflectiveClass = (ReflectiveClass) value;
                return reflectiveClass.Instance;
            }

            throw new Exception("Unable to convert value");
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        public static IEnumerable<T> GenerateEnumerable<T>(object value)
        {
            List<T> items = new List<T>();

            bool isReflective = ImplementsOrInherits(typeof(T), typeof(ReflectiveClass));

            IEnumerable enumerable = (IEnumerable) value;
            foreach (object obj in enumerable)
            {
                T item = default;

                if (isReflective)
                {
                    item = (T) Activator.CreateInstance(typeof(T), obj);
                }
                else
                {
                    item = (T) obj;
                }

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        public static T[] GenerateArray<T>(object value)
        {
            return GenerateEnumerable<T>(value)
                .ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        public static IDictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(object data)
        {
            IDictionary dict = (IDictionary) data;
            return GenerateDictionary<TKey, TValue>(dict);
        }

        private static IDictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(IDictionary dictionary)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            bool keyIsReflective = ImplementsOrInherits(typeof(TKey), typeof(ReflectiveClass));
            bool valueIsReflective = ImplementsOrInherits(typeof(TValue), typeof(ReflectiveClass));

            foreach (DictionaryEntry entry in dictionary)
            {
                TKey key = default;
                TValue value = default;

                if (keyIsReflective)
                {
                    object instance = Activator.CreateInstance(typeof(TKey), entry.Key);
                    key = (TKey) instance;
                }
                else
                {
                    key = (TKey) entry.Key;
                }

                if (valueIsReflective)
                {
                    object instance = Activator.CreateInstance(typeof(TValue), entry.Value);
                    value = (TValue) instance;
                }
                else
                {
                    value = (TValue) entry.Value;
                }

                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// Checks if the given type implements or inherits another type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="implementsOrInherits">The type it needs to implement or inherit</param>
        /// <returns>true if it implements or inherits, otherwise false</returns>
        [PublicAPI]
        public static bool ImplementsOrInherits(Type type, Type implementsOrInherits)
        {
            if (implementsOrInherits.IsAssignableFrom(type))
                return true;

            if (type == implementsOrInherits)
                return true;

            if (type.BaseType == null)
                return false;

            return ImplementsOrInherits(type.BaseType, implementsOrInherits);
        }

        internal static bool IsPublic(Type type)
        {
            if (type.IsArray)
                return IsPublic(type.GetElementType());

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                if (type.IsNested)
                {
                    return type.IsNestedPublic && type.DeclaringType.IsPublic;
                }

                return type.IsPublic;
            }

            return type.IsPublic && genericArguments.All(IsPublic);
        }
    }
}
