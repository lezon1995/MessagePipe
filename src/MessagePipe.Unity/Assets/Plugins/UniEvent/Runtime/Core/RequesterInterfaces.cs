using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniEvent
{
    public interface IRequesterHandler<in T, R>
    {
        bool TryHandle(T message, out R result);
        (bool, UniTask<R>) TryHandleAsync(T message);
        (bool, UniTask<R>) TryHandleAsync(T message, CancellationToken token);
    }

    public interface IEventRequester<T, R>
    {
        bool TryPublish(T message, out R result);
        UniTask<(bool, R)> TryPublishAsync(T message, CancellationToken token = default);
        UniTask<(bool, R)> TryPublishAsync(T message, PublishAsyncStrategy strategy, CancellationToken token = default);
        UniTask<bool> TryPublishAsync(T message, List<R> result, CancellationToken token = default);
        UniTask<bool> TryPublishAsync(T message, List<R> result, PublishAsyncStrategy strategy, CancellationToken token = default);
        IDisposable Subscribe(IRequesterHandler<T, R> handler);
    }

    public interface ITopicRequester<in K, T, R>
    {
        bool TryPublish(K key, T message, out R result);
        UniTask<(bool, R)> TryPublishAsync(K key, T message, CancellationToken token = default);
        UniTask<(bool, R)> TryPublishAsync(K key, T message, PublishAsyncStrategy strategy, CancellationToken token = default);
        UniTask<bool> TryPublishAsync(K key, T message, List<R> result, CancellationToken token = default);
        UniTask<bool> TryPublishAsync(K key, T message, List<R> result, PublishAsyncStrategy strategy, CancellationToken token = default);
        IDisposable Subscribe(K key, IRequesterHandler<T, R> handler);
    }
}