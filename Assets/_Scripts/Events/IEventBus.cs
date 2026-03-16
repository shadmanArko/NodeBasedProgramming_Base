using System;

namespace _Scripts.Events
{
    public interface IEventBus
    {
        IObservable<T> OnEvent<T>();
        void Publish<T>(T evt);
    }
}