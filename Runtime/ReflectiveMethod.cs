using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class ReflectiveMethod
    {
        private readonly object instance;
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        /// <param name="types"></param>
        [PublicAPI]
        public ReflectiveMethod(Type definingType, string name, BindingFlags flags, object instance, Type[] types = null)
        {
            this.instance = instance;
            if (types == null)
            {
                methodInfo = definingType.GetMethod(name, flags);
            }
            else
            {
                methodInfo = GetMethodInfoBasedOnParameters(definingType, name, flags, types);
            }
        }

        private MethodInfo GetMethodInfoBasedOnParameters(Type definingType, string name, BindingFlags flags, Type[] types)
        {
            MethodInfo[] methods = definingType.GetMethods(flags)
                .Where(x => x.Name == name)
                .ToArray();

            if (methods.Length == 1)
            {
                return methods.First();
            }

            methods = methods.Where(x => x.GetParameters().Length == types.Length)
                .ToArray();

            if (methods.Length == 1)
            {
                return methods.First();
            }

            return FindInMethods(methods, types);
        }

        private MethodInfo FindInMethods(MethodInfo[] methods, Type[] types)
        {
            foreach (MethodInfo method in methods)
            {
                if (DoParametersMatch(method, types))
                {
                    return method;
                }
            }

            return null;
        }

        private bool DoParametersMatch(MethodInfo method, Type[] types)
        {
            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type parameterType = parameter.ParameterType;
                Type expectedType = types[i];

                if (parameterType == expectedType)
                {
                    continue;
                }

                if (IsMatchingEnumParameter(expectedType, parameterType))
                {
                    continue;
                }

                if (Utilities.ImplementsOrInherits(expectedType, typeof(ReflectiveClass)) && parameterType.Name == expectedType.Name)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool IsMatchingEnumParameter(Type expectedType, Type parameterType)
        {
            if (!expectedType.IsEnum || !parameterType.IsEnum)
            {
                return true;
            }

            return expectedType.GetEnumUnderlyingType() == parameterType.GetEnumUnderlyingType() && expectedType.Name == parameterType.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [PublicAPI]
        public object Invoke(params object[] parameters)
        {
            return methodInfo.Invoke(instance, parameters);
        }
    }
}