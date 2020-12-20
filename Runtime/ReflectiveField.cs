using System;
using System.Reflection;

namespace TNRD.Reflectives
{
    public class ReflectiveField
    {
        private readonly object instance;
        private readonly FieldInfo fieldInfo;

        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            fieldInfo = definingType.GetField(name, flags);
        }

        public void SetValue(object value)
        {
            fieldInfo.SetValue(instance, value);
        }

        public object GetValue()
        {
            return fieldInfo.GetValue(instance);
        }
    }

    public class ReflectiveField<T> : ReflectiveField
    {
        public ReflectiveField(Type definingType, string name, BindingFlags flags, object instance) : base(definingType, name, flags, instance)
        {
        }

        public new T GetValue()
        {
            return (T) base.GetValue();
        }
    }
}