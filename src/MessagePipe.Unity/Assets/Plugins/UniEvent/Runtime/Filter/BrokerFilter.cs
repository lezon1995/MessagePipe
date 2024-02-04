using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniEvent.Internal;

namespace UniEvent
{
    public interface IFilterAttribute
    {
        Type Type { get; }
        int Order { get; }
    }

    public interface IHandlerFilter
    {
        public int Order { get; set; }
    }

    // Sync/Async filter
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [Preserve]
    public class BrokerFilterAttribute : Attribute, IFilterAttribute
    {
        public Type Type { get; }
        public int Order { get; set; }

        [Preserve]
        public BrokerFilterAttribute(Type type)
        {
            if (typeof(IBrokerHandlerFilter).IsAssignableFrom(type))
            {
                Type = type;
            }
            else
            {
                throw new ArgumentException($"{type.FullName} is not BrokerFilterAttribute.");
            }
        }
    }

    public interface IBrokerHandlerFilter : IHandlerFilter
    {
    }

    public abstract class BrokerHandlerFilter<T> : IBrokerHandlerFilter
    {
        public int Order { get; set; }
        public abstract void Handle(T message, Action<T> next);
        public abstract UniTask HandleAsync(T message, Func<T, UniTask> next);
        public abstract UniTask HandleAsync(T message, CancellationToken token, Func<T, CancellationToken, UniTask> next);
    }
}