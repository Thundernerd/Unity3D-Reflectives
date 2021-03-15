using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// A couple of utilities that can be used with Reflectives
    /// </summary>
    [PublicAPI]
    public static class Utilities
    {
        /// <summary>
        /// 
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
        /// 
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
    }
}
