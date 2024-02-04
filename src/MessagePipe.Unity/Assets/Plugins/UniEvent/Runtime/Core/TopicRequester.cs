using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniEvent.Internal;

namespace UniEvent
{
    [Preserve]
    public class TopicRequester<K, T, R> : ITopicRequester<K, T, R>, IDisposable, IHandlerHolderMarker
    {
        Options options;
        HandlerFactory handlerFactory;
        DiagnosticsInfo diagnosticsInfo;

        Dictionary<K, HandlerHolder> handlerGroup;
        object gate;
        bool isDisposed;

        [Preserve]
        public TopicRequester(Options _options, HandlerFactory _handlerFactory, DiagnosticsInfo _diagnosticsInfo)
        {
            options = _options;
            handlerFactory = _handlerFactory;
            diagnosticsInfo = _diagnosticsInfo;

            handlerGroup = new Dictionary<K, HandlerHolder>();
            gate = new object();
        }

        public bool TryPublish(K key, T message, out R result)
        {
            List<IRequesterHandler<T, R>> handlers;
            lock (gate)
            {
                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    result = default;
                    return false;
                }

                handlers = holder.GetHandlers();
            }

            foreach (var handler in handlers)
            {
                if (handler != null)
                {
                    if (handler.TryHandle(message, out result))
                    {
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        public bool TryPublish(K key, T message, List<R> list)
        {
            list.Clear();
            List<IRequesterHandler<T, R>> handlers;
            lock (gate)
            {
                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    return false;
                }

                handlers = holder.GetHandlers();
            }

            foreach (var handler in handlers)
            {
                if (handler != null)
                {
                    if (handler.TryHandle(message, out var result))
                    {
                        list.Add(result);
                        return true;
                    }
                }
            }

            return false;
        }

        public async UniTask<(bool, R)> TryPublishAsync(K key, T message, CancellationToken token = default)
        {
            return await TryPublishAsync(key, message, options.DefaultPublishAsyncStrategy, token);
        }

        public async UniTask<(bool, R)> TryPublishAsync(K key, T message, PublishAsyncStrategy strategy, CancellationToken token = default)
        {
            List<IRequesterHandler<T, R>> handlers;
            int count;
            lock (gate)
            {
                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    return (false, default);
                }

                handlers = holder.GetHandlers();
                count = holder.GetCount();
            }

            if (count <= 1 || strategy == PublishAsyncStrategy.Sequential)
            {
                foreach (var handler in handlers)
                {
                    bool success;
                    UniTask<R> task;
                    if (token == default)
                        (success, task) = handler.TryHandleAsync(message);
                    else
                        (success, task) = handler.TryHandleAsync(message, token);

                    if (success)
                    {
                        var result = await task;
                        return (true, result);
                    }
                }
            }
            else
            {
                var results = await new AsyncHandlerWhenAll<T, R>(handlers, message, token);
                if (results.Length > 0)
                {
                    return (true, results[0]);
                }
            }

            return (false, default);
        }

        public async UniTask<bool> TryPublishAsync(K key, T message, List<R> result, CancellationToken token = default)
        {
            return await TryPublishAsync(key, message, result, options.DefaultPublishAsyncStrategy, token);
        }

        public async UniTask<bool> TryPublishAsync(K key, T message, List<R> list, PublishAsyncStrategy strategy, CancellationToken token = default)
        {
            list.Clear();
            List<IRequesterHandler<T, R>> handlers;
            int count;
            lock (gate)
            {
                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    return false;
                }

                handlers = holder.GetHandlers();
                count = holder.GetCount();
            }

            bool hasResult = false;
            if (count <= 1 || strategy == PublishAsyncStrategy.Sequential)
            {
                foreach (var handler in handlers)
                {
                    bool success;
                    UniTask<R> task;
                    if (token == default)
                        (success, task) = handler.TryHandleAsync(message);
                    else
                        (success, task) = handler.TryHandleAsync(message, token);

                    if (success)
                    {
                        var result = await task;
                        list.Add(result);
                        hasResult = true;
                    }
                }
            }
            else
            {
                var results = await new AsyncHandlerWhenAll<T, R>(handlers, message, token);
                list.AddRange(results);
                hasResult = true;
            }

            return hasResult;
        }


        public IDisposable Subscribe(K key, IRequesterHandler<T, R> handler)
        {
            lock (gate)
            {
                if (isDisposed)
                {
                    return options.HandlingSubscribeDisposedPolicy.Handle(nameof(TopicRequester<K, T, R>));
                }

                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    handlerGroup[key] = holder = new HandlerHolder(this);
                }

                // handler = handlerFactory.BuildHandler(handler, filters);

                return holder.Subscribe(key, handler);
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    foreach (var handlers in handlerGroup.Values)
                    {
                        handlers.Dispose();
                    }
                }
            }
        }

        sealed class HandlerHolder : IDisposable, IHandlerHolderMarker
        {
            List<IRequesterHandler<T, R>> handlers;
            TopicRequester<K, T, R> requester;

            public HandlerHolder(TopicRequester<K, T, R> _requester)
            {
                handlers = new List<IRequesterHandler<T, R>>();
                requester = _requester;
            }

            public List<IRequesterHandler<T, R>> GetHandlers()
            {
                return handlers;
            }

            public int GetCount()
            {
                return handlers.Count;
            }

            public IDisposable Subscribe(K key, IRequesterHandler<T, R> handler)
            {
                handlers.Add(handler);
                var subscription = new Subscription(key, handler, this);
                requester.diagnosticsInfo.IncrementSubscribe(this, subscription);
                return subscription;
            }

            public void Dispose()
            {
                lock (requester.gate)
                {
                    var count = handlers.Count;
                    handlers.Clear();
                    requester.diagnosticsInfo.RemoveTargetDiagnostics(this, count);
                }
            }

            sealed class Subscription : IDisposable
            {
                bool isDisposed;
                K key;
                IRequesterHandler<T, R> subscriptionKey;
                HandlerHolder holder;

                public Subscription(K _key, IRequesterHandler<T, R> _subscriptionKey, HandlerHolder _holder)
                {
                    key = _key;
                    subscriptionKey = _subscriptionKey;
                    holder = _holder;
                }

                public void Dispose()
                {
                    if (!isDisposed)
                    {
                        isDisposed = true;
                        lock (holder.requester.gate)
                        {
                            if (!holder.requester.isDisposed)
                            {
                                holder.handlers.Remove(subscriptionKey);
                                holder.requester.diagnosticsInfo.DecrementSubscribe(holder, this);
                                if (holder.handlers.Count == 0)
                                {
                                    holder.requester.handlerGroup.Remove(key);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}