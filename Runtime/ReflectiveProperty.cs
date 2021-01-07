using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// A wrapper around a property that is accessed through reflection
    /// </summary>
    [PublicAPI]
    public class ReflectiveProperty
    {
        private PropertyInfo propertyInfo;
        private object instance;

        /// <summary>
        /// Creates a new reflective property
        /// </summary>
        /// <param name="definingType">The type that defines this property</param>
        /// <param name="name">The name of the property</param>
        /// <param name="flags">The binding flags that should be used to find the property</param>
        /// <param name="instance">The instance that can be used for getting and/or setting the value</param>
        [PublicAPI]
        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            propertyInfo = definingType.GetProperty(name, flags);
        }

        /// <summary>
        /// Sets the value on the instance
        /// </summary>
        /// <param name="value">The value to set</param>
        [PublicAPI]
        public void SetValue(object value)
        {
            propertyInfo.SetValue(instance, value);
        }

        /// <summary>
        /// Gets the value from the instance
        /// </summary>
        /// <returns>The value as an object</returns>
        [PublicAPI]
        public object GetValue()
        {
            return propertyInfo.GetValue(instance);
        }
    }

    /// <summary>
    /// A typed wrapper around a property that is accessed through reflection
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    [PublicAPI]
    public class ReflectiveProperty<T> : ReflectiveProperty
    {
        /// <inheritdoc/>
        [PublicAPI]
        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
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