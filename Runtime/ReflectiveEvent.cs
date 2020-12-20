using System;
using System.Reflection;

namespace TNRD.Reflectives
{
    public class ReflectiveEvent
    {
        private readonly object instance;
        private readonly EventInfo eventInfo;

        public ReflectiveEvent(Type definingType, string name, BindingFlags flags, object instance)
        {
            this.instance = instance;
            eventInfo = definingType.GetEvent(name, flags);
        }

        public Delegate Subscribe(Delegate @delegate)
        {
            Delegate convertedDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, @delegate.Target, @delegate.Method);
            eventInfo.AddEventHandler(instance, convertedDelegate);
            return convertedDelegate;
        }

        public void Unsubscribe(Delegate @delegate)
        {
            eventInfo.RemoveEventHandler(instance, @delegate);
        }
    }
}