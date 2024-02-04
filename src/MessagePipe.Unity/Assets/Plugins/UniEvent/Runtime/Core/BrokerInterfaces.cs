using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniEvent
{
    public interface IBrokerHandler<in T>
    {
        void Handle(T message);
        UniTask HandleAsync(T message);
        UniTask HandleAsync(T message, CancellationToken token);
    }

    public interface IEventBroker<T>
    {
        void Publish(T message, bool buffered = false);
        UniTask PublishAsync(T message, bool buffered = false, CancellationToken token = default);
        UniTask PublishAsync(T message, PublishAsyncStrategy strategy, bool buffered = false, CancellationToken token = default);

        IDisposable Subscribe(IBrokerHandler<T> handler, bool handleBuffered = false, params BrokerHandlerFilter<T>[] filters);
        UniTask<IDisposable> SubscribeAsync(IBrokerHandler<T> handler, bool handleBuffered = false, CancellationToken token = default, params BrokerHandlerFilter<T>[] filters);
    }

    public interface ITopicBroker<in K, T>
    {
        void Publish(K key, T message);
        UniTask PublishAsync(K key, T message, CancellationToken token = default);
        UniTask PublishAsync(K key, T message, PublishAsyncStrategy strategy, CancellationToken token = default);

        IDisposable Subscribe(K key, IBrokerHandler<T> handler, params BrokerHandlerFilter<T>[] filters);
    }
}