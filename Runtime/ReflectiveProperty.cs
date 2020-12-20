using System;
using System.Reflection;

namespace TNRD.Reflectives
{
    public class ReflectiveProperty
    {
        private PropertyInfo propertyInfo;
        private object instance;

        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            propertyInfo = definingType.GetProperty(name, flags);
        }

        public void SetValue(object value)
        {
            propertyInfo.SetValue(instance, value);
        }

        public object GetValue()
        {
            return propertyInfo.GetValue(instance);
        }
    }

    public class ReflectiveProperty<T> : ReflectiveProperty
    {
        public ReflectiveProperty(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
        {
        }

        public new T GetValue()
        {
            return (T) base.GetValue();
        }
    }
}