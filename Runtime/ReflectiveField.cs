using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// A wrapper around a field that is accessed through reflection
    /// </summary>
    [PublicAPI]
    public class ReflectiveField
    {
        private readonly object instance;
        private readonly FieldInfo fieldInfo;

        /// <summary>
        /// Creates a new reflective field
        /// </summary>
        /// <param name="definingType">The type that defines this field</param>
        /// <param name="name">The name of the field</param>
        /// <param name="flags">The binding flags that should be used to find the field</param>
        /// <param name="instance">The instance that can be used for getting and/or setting the value</param>
        [PublicAPI]
        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            fieldInfo = definingType.GetField(name, flags);
        }

        /// <summary>
        /// Sets the value on the instance
        /// </summary>
        /// <param name="value">The value to set</param>
        [PublicAPI]
        public void SetValue(object value)
        {
            fieldInfo.SetValue(instance, value);
        }

        /// <summary>
        /// Gets the value from the instance
        /// </summary>
        /// <returns>The value as an object</returns>
        [PublicAPI]
        public object GetValue()
        {
            return fieldInfo.GetValue(instance);
        }
    }

    /// <summary>
    /// A typed wrapper around a field that is access through reflection
    /// </summary>
    /// <typeparam name="T">The type of the field</typeparam>
    [PublicAPI]
    public class ReflectiveField<T> : ReflectiveField
    {
        /// <inheritdoc/>
        [PublicAPI]
        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
        {
        }

        /// <summary>
        /// Gets the typed value from the instance
        /// </summary>
        /// <returns>The value as the generic type</returns>
        [PublicAPI]
        public new T GetValue()
        {
            return (T) base.GetValue();
        }
    }
}