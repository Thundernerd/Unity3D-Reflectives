using System;
using System.Reflection;

namespace TNRD.Reflectives
{
    public class ReflectiveMethod
    {
        private readonly object instance;
        private readonly MethodInfo methodInfo;

        public ReflectiveMethod(
            Type definingType,
            string name,
            BindingFlags flags,
            object instance,
            Type[] types = null
        )
        {
            this.instance = instance;
            if (types == null)
            {
                methodInfo = definingType.GetMethod(name, flags);
            }
            else
            {
                methodInfo = definingType.GetMethod(name, flags, null, CallingConventions.Any, types, null);
            }
        }

        public object Invoke(params object[] parameters)
        {
            return methodInfo.Invoke(instance, parameters);
        }
    }
}