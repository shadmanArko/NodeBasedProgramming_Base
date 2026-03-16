using System;
using System.Collections.Generic;
using UniRx;

namespace _Scripts.Events
{
    public sealed class SimpleEventBus : IEventBus, IDisposable
    {
        private readonly Dictionary<Type, object> _subjects = new Dictionary<Type, object>();

        public IObservable<T> OnEvent<T>()
        {
            var type = typeof(T);
            if (!_subjects.TryGetValue(type, out var raw))
            {
                var subject = new Subject<T>();
                _subjects[type] = subject;
                return subject;
            }

            return (Subject<T>)raw;
        }

        public void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (_subjects.TryGetValue(type, out var raw))
                ((Subject<T>)raw).OnNext(evt);
        }

        public void Dispose()
        {
            foreach (var subject in _subjects.Values)
                (subject as IDisposable)?.Dispose();
            _subjects.Clear();
        }
    }
}