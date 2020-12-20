using System;
using System.Collections;
using System.Collections.Generic;

namespace TNRD.Reflectives
{
    public static class Utilities
    {
        private static TEnum GetEnum<TEnum, TBase>(ReflectiveField reflectiveField)
            where TEnum : Enum
        {
            object value = (TBase) reflectiveField.GetValue();
            return (TEnum) value;
        }

        public static TEnum GetIntEnum<TEnum>(ReflectiveField reflectiveField)
            where TEnum : Enum
        {
            return GetEnum<TEnum, int>(reflectiveField);
        }

        public static TEnum GetUIntEnum<TEnum>(ReflectiveField reflectiveField)
            where TEnum : Enum
        {
            return GetEnum<TEnum, uint>(reflectiveField);
        }

        private static TEnum GetEnum<TEnum, TBase>(ReflectiveProperty reflectiveField)
            where TEnum : Enum
        {
            object value = (TBase) reflectiveField.GetValue();
            return (TEnum) value;
        }

        public static TEnum GetIntEnum<TEnum>(ReflectiveProperty reflectiveField)
            where TEnum : Enum
        {
            return GetEnum<TEnum, int>(reflectiveField);
        }

        public static TEnum GetUIntEnum<TEnum>(ReflectiveProperty reflectiveField)
            where TEnum : Enum
        {
            return GetEnum<TEnum, uint>(reflectiveField);
        }

        public static List<T> GenerateEnumerable<T>(object value) where T : ReflectiveClass
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

        public static Dictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(ReflectiveField reflectiveField)
        {
            IDictionary dict = (IDictionary) reflectiveField.GetValue();
            return GenerateDictionary<TKey, TValue>(dict);
        }

        public static Dictionary<TKey, TValue> GenerateDictionary<TKey, TValue>(ReflectiveProperty reflectiveProperty)
        {
            IDictionary dict = (IDictionary) reflectiveProperty.GetValue();
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