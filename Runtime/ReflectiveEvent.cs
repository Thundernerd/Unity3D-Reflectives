using System;
using System.Reflection;
using JetBrains.Annotations;

namespace TNRD.Reflectives
{
    /// <summary>
    /// A wrapper around an event that is access through reflection
    /// </summary>
    [PublicAPI]
    public class ReflectiveEvent
    {
        private readonly object instance;
        private readonly EventInfo eventInfo;

        /// <summary>
        /// Creates a new reflective event
        /// </summary>
        /// <param name="definingType">The type that defines this event</param>
        /// <param name="name">The name of the event</param>
        /// <param name="flags">The binding flags that should be used to find the event</param>
        /// <param name="instance">The instance that can be used for subscribing to and/or unsubscribing from the event</param>
        [PublicAPI]
        public ReflectiveEvent(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            eventInfo = definingType.GetEvent(name, flags);
        }

        /// <summary>
        /// Subscribes the given delegate to the event
        /// </summary>
        /// <param name="delegate">The delegate to subscribe to the event</param>
        /// <returns>A converted delegate that matches the definition of the actual event. Used for unsubscribing</returns>
        [PublicAPI]
        public Delegate Subscribe(Delegate @delegate)
        {
            Delegate convertedDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, @delegate.Target, @delegate.Method);
            eventInfo.AddEventHandler(instance, convertedDelegate);
            return convertedDelegate;
        }

        /// <summary>
        /// Unsubscribes the given delegate from the event
        /// </summary>
        /// <param name="delegate">The delegate received from <see cref="Subscribe"/> when registering</param>
        [PublicAPI]
        public void Unsubscribe(Delegate @delegate)
        {
            eventInfo.RemoveEventHandler(instance, @delegate);
        }
    }
}