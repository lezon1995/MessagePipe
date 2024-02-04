using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniEvent
{
    public static partial class SubscriberExtensions
    {
        public static IObservable<T> AsObservable<T>(this IEventBroker<T> subscriber, params BrokerHandlerFilter<T>[] filters)
        {
            return new ObservableSubscriber<T>(subscriber, filters);
        }

        public static IObservable<T> AsObservable<K, T>(this ITopicBroker<K, T> subscriber, K key, params BrokerHandlerFilter<T>[] filters)

        {
            return new ObservableSubscriber<K, T>(key, subscriber, filters);
        }
    }

    internal sealed class ObservableSubscriber<K, T> : IObservable<T>
    {
        K key;
        ITopicBroker<K, T> subscriber;
        BrokerHandlerFilter<T>[] filters;

        public ObservableSubscriber(K _key, ITopicBroker<K, T> _subscriber, BrokerHandlerFilter<T>[] _filters)
        {
            key = _key;
            subscriber = _subscriber;
            filters = _filters;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return subscriber.Subscribe(key, new ObserverHandler<T>(observer), filters);
        }
    }

    internal sealed class ObservableSubscriber<T> : IObservable<T>
    {
        IEventBroker<T> subscriber;
        BrokerHandlerFilter<T>[] filters;

        public ObservableSubscriber(IEventBroker<T> _subscriber, BrokerHandlerFilter<T>[] _filters)
        {
            subscriber = _subscriber;
            filters = _filters;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return subscriber.Subscribe(new ObserverHandler<T>(observer), false, filters);
        }
    }


    internal sealed class ObserverHandler<T> : IBrokerHandler<T>
    {
        IObserver<T> observer;

        public ObserverHandler(IObserver<T> _observer)
        {
            observer = _observer;
        }

        public void Handle(T message)
        {
            observer.OnNext(message);
        }

        public UniTask HandleAsync(T message)
        {
            throw new NotImplementedException();
        }

        public UniTask HandleAsync(T message, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}