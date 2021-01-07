using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class ReflectiveProperty
    {
        private PropertyInfo propertyInfo;
        private object instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        [PublicAPI]
        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            propertyInfo = definingType.GetProperty(name, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        [PublicAPI]
        public void SetValue(object value)
        {
            propertyInfo.SetValue(instance, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public object GetValue()
        {
            return propertyInfo.GetValue(instance);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public class ReflectiveProperty<T> : ReflectiveProperty
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        [PublicAPI]
        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public new T GetValue()
        {
            return (T) base.GetValue();
        }
    }
}