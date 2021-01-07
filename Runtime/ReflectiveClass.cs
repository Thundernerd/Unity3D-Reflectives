using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace TNRD.Reflectives
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class ReflectiveClass
    {
        private const BindingFlags DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly object instance;
        private readonly Type type;

        /// <summary>
        /// 
        /// </summary>
        [PublicAPI]
        public object Instance => instance;
        /// <summary>
        /// 
        /// </summary>
        [PublicAPI]
        public Type Type => type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public ReflectiveClass(object instance)
        {
            if (instance == null)
                return;

            this.instance = instance;
            this.type = instance.GetType();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ReflectiveClass(Type type)
        {
            this.instance = null;
            this.type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveField CreateField(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField<T>(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty<T>(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveMethod(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS, params Type[] types)
        {
            return new ReflectiveMethod(type, name, flags, instance, types);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveEvent CreateEvent(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveEvent(type, name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public object NewInstance()
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [PublicAPI]
        public object NewInstance(params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    [PublicAPI]
    public class ReflectiveClass<TClass> where TClass : new()
    {
        private readonly TClass instance;

        /// <summary>
        /// 
        /// </summary>
        [PublicAPI]
        protected TClass Instance => instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        [PublicAPI]
        protected ReflectiveClass(TClass instance)
        {
            this.instance = instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveField CreateField(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField(typeof(TClass), name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField<T>(typeof(TClass), name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty(typeof(TClass), name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty<T>(typeof(TClass), name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveMethod(typeof(TClass), name, flags, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reflectiveClass"></param>
        /// <returns></returns>
        [PublicAPI]
        public static implicit operator TClass(ReflectiveClass<TClass> reflectiveClass)
        {
            return reflectiveClass.instance;
        }
    }
}