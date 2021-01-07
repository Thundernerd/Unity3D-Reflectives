using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace TNRD.Reflectives
{
    /// <summary>
    /// A wrapper around a class that can be used to easily created reflected events, fields, properties, and methods
    /// </summary>
    [PublicAPI]
    public class ReflectiveClass
    {
        /// <summary>
        /// The default binding flags to find members with
        /// </summary>
        [PublicAPI]
        public const BindingFlags DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly object instance;
        private readonly Type type;

        /// <summary>
        /// The original instance that the members will use for reflection
        /// </summary>
        [PublicAPI]
        public object Instance => instance;

        /// <summary>
        /// The type of the reflected class 
        /// </summary>
        [PublicAPI]
        public Type Type => type;

        /// <summary>
        /// Creates a new reflective class
        /// </summary>
        /// <param name="instance">The instance that will be used for reflection</param>
        public ReflectiveClass(object instance)
        {
            if (instance == null)
                return;

            this.instance = instance;
            this.type = instance.GetType();
        }

        /// <summary>
        /// Creates a new reflective class. Use this for a class with static members
        /// </summary>
        /// <param name="type">The type that describes this reflective class</param>
        public ReflectiveClass(Type type)
        {
            this.instance = null;
            this.type = type;
        }

        /// <summary>
        /// Creates a new reflective field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="flags">The binding flags that should be used to find the field. See <see cref="DEFAULT_FLAGS"/></param>
        /// <returns>A reflective field</returns>
        [PublicAPI]
        protected ReflectiveField CreateField(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new typed reflective field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="flags">The binding flags that should be used to find the field. See <see cref="DEFAULT_FLAGS"/></param>
        /// <typeparam name="T">The type of the field</typeparam>
        /// <returns>A typed reflective field</returns>
        [PublicAPI]
        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveField<T>(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new reflective property
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="flags">The binding flags that should be used to find the property. See <see cref="DEFAULT_FLAGS"/></param>
        /// <returns>A reflective property</returns>
        [PublicAPI]
        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new typed reflective property
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="flags">The binding flags that should be used to find the property. See <see cref="DEFAULT_FLAGS"/></param>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <returns>A typed reflective property</returns>
        [PublicAPI]
        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveProperty<T>(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new reflective method
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="flags">The binding flags that should be used to find the method. See <see cref="DEFAULT_FLAGS"/></param>
        /// <returns>A reflective method</returns>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveMethod(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new reflective method
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="flags">The binding flags that should be used to find the method. See <see cref="DEFAULT_FLAGS"/></param>
        /// <param name="types">The types of the parameters the method should have, used for finding the method if there are overloads</param>
        /// <returns>A reflective method</returns>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = DEFAULT_FLAGS, params Type[] types)
        {
            return new ReflectiveMethod(type, name, flags, instance, types);
        }

        /// <summary>
        /// Creates a new reflective event
        /// </summary>
        /// <param name="name">The name of the event</param>
        /// <param name="flags">The binding flags that should be used to find the event. See <see cref="DEFAULT_FLAGS"/></param>
        /// <returns>A reflective event</returns>
        [PublicAPI]
        protected ReflectiveEvent CreateEvent(string name, BindingFlags flags = DEFAULT_FLAGS)
        {
            return new ReflectiveEvent(type, name, flags, instance);
        }

        /// <summary>
        /// Creates a new instance based on the type this class reflects
        /// </summary>
        /// <returns>A new instance of the type this class reflects</returns>
        [PublicAPI]
        public object NewInstance()
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates a new instanced based on the type this class reflects
        /// </summary>
        /// <param name="args">Parameters that are used for the constructor</param>
        /// <returns>A new instance of the type this class reflects</returns>
        [PublicAPI]
        public object NewInstance(params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
    }

    /// <summary>
    /// A typed wrapper around a class that can be used to easily created reflected events, fields, properties, and methods
    /// </summary>
    /// <typeparam name="TClass">The type of the class to wrap around</typeparam>
    [PublicAPI]
    public class ReflectiveClass<TClass> where TClass : new()
    {
        private readonly TClass instance;

        /// <summary>
        /// The original typed instance that the members will use for reflection
        /// </summary>
        [PublicAPI]
        protected TClass Instance => instance;

        /// <summary>
        /// Creates a new reflective class
        /// </summary>
        /// <param name="instance">The instance that will be used for reflection</param>
        [PublicAPI]
        public ReflectiveClass(TClass instance)
        {
            this.instance = instance;
        }

        /// <inheritdoc cref="ReflectiveClass.CreateField"/>
        [PublicAPI]
        protected ReflectiveField CreateField(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField(typeof(TClass), name, flags, instance);
        }

        /// <inheritdoc cref="ReflectiveClass.CreateField"/>
        /// <typeparam name="T">The type of the field</typeparam>
        /// <returns>A typed reflective field</returns>
        [PublicAPI]
        protected ReflectiveField<T> CreateField<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveField<T>(typeof(TClass), name, flags, instance);
        }

        /// <inheritdoc cref="ReflectiveClass.CreateProperty"/>
        [PublicAPI]
        protected ReflectiveProperty CreateProperty(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty(typeof(TClass), name, flags, instance);
        }

        /// <inheritdoc cref="ReflectiveClass.CreateProperty"/>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <returns>A typed reflective property</returns>
        [PublicAPI]
        protected ReflectiveProperty<T> CreateProperty<T>(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveProperty<T>(typeof(TClass), name, flags, instance);
        }

        /// <inheritdoc cref="ReflectiveClass.CreateMethod(string,System.Reflection.BindingFlags)"/>
        [PublicAPI]
        protected ReflectiveMethod CreateMethod(string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return new ReflectiveMethod(typeof(TClass), name, flags, instance);
        }

        [PublicAPI]
        public static implicit operator TClass(ReflectiveClass<TClass> reflectiveClass)
        {
            return reflectiveClass.instance;
        }
    }
}