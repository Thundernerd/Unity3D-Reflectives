using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class ReflectiveField
    {
        private readonly object instance;
        private readonly FieldInfo fieldInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        [PublicAPI]
        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            fieldInfo = definingType.GetField(name, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        [PublicAPI]
        public void SetValue(object value)
        {
            fieldInfo.SetValue(instance, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public object GetValue()
        {
            return fieldInfo.GetValue(instance);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public class ReflectiveField<T> : ReflectiveField
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        [PublicAPI]
        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
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