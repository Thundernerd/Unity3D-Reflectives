using System;
using System.Collections;
using System.Collections.Generic;

namespace TNRD.Reflectives
{
    public static class Utilities
    {
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

        public static Dictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(object data)
        {
            IDictionary dict = (IDictionary) data;
            return GenerateDictionary<TKey, TValue>(dict);
        }

        private static Dictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(IDictionary dictionary)
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

        private static bool ImplementsOrInherits(Type type, Type implementsOrInherits)
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