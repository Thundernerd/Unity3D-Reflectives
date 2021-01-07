using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class ReflectiveEvent
    {
        private readonly object instance;
        private readonly EventInfo eventInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definingType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="instance"></param>
        [PublicAPI]
        public ReflectiveEvent(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            eventInfo = definingType.GetEvent(name, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegate"></param>
        /// <returns></returns>
        [PublicAPI]
        public Delegate Subscribe(Delegate @delegate)
        {
            Delegate convertedDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, @delegate.Target, @delegate.Method);
            eventInfo.AddEventHandler(instance, convertedDelegate);
            return convertedDelegate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegate"></param>
        [PublicAPI]
        public void Unsubscribe(Delegate @delegate)
        {
            eventInfo.RemoveEventHandler(instance, @delegate);
        }
    }
}