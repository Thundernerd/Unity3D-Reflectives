using System;
using System.Reflection;
using UnityEngine.Assertions;

namespace TNRD.Reflectives
{
    public class ReflectiveClass
    {
        private const BindingFlags DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly object instance;
        private readonly Type type;

        public object Instance => instance;
        public Type Type => type;

        public ReflectiveClass(object instance)
        {
            if (instance == null)
                return;

            this.instance = instance;
            this.type = instance.GetType();
        }

        public ReflectiveClass(Type type)
        {
            this.instance = null;
            this.type = type;
        }

        protected ReflectiveField CreateField(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField(type, name, flags, instance);
        }

        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField<T>(type, name, flags, instance);
        }

        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty(type, name, flags, instance);
        }

        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty<T>(type, name, flags, instance);
        }

        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveMethod(type, name, flags, instance);
        }

        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS, params Type[] types)
        {
            return new ReflectiveMethod(type, name, flags, instance, types);
        }

        protected ReflectiveEvent CreateEvent(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveEvent(type, name, flags, instance);
        }

        public object NewInstance()
        {
            return Activator.CreateInstance(type);
        }

        public object NewInstance(params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
    }

    public class ReflectiveClass<TClass> where TClass : new()
    {
        private readonly TClass instance;

        protected TClass Instance => instance;

        protected ReflectiveClass(TClass instance)
        {
            this.instance = instance;
        }

        protected ReflectiveField CreateField(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField(typeof(TClass), name, flags, instance);
        }

        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField<T>(typeof(TClass), name, flags, instance);
        }

        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty(typeof(TClass), name, flags, instance);
        }

        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty<T>(typeof(TClass), name, flags, instance);
        }

        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveMethod(typeof(TClass), name, flags, instance);
        }

        public static implicit operator TClass(ReflectiveClass<TClass> reflectiveClass)
        {
            return reflectiveClass.instance;
        }
    }
}