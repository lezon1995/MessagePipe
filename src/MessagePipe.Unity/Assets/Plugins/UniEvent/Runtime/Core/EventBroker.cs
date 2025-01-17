using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniEvent.Internal;

namespace UniEvent
{
    [Preserve]
    public class EventBroker<T> : IEventBroker<T>, IDisposable, IHandlerHolderMarker
    {
        Options options;
        HandlerFactory handlerFactory;
        DiagnosticsInfo diagnosticsInfo;

        List<IBrokerHandler<T>> handlers;
        object gate;
        bool isDisposed;

        Queue<T> buffer => _buffer ??= new Queue<T>();
        Queue<T> _buffer;

        [Preserve]
        public EventBroker(Options _options, HandlerFactory _handlerFactory, DiagnosticsInfo _diagnosticsInfo)
        {
            options = _options;
            handlerFactory = _handlerFactory;
            diagnosticsInfo = _diagnosticsInfo;

            handlers = new List<IBrokerHandler<T>>();
            gate = new object();
        }

        public void Publish(T message, bool buffered = false)
        {
            if (buffered)
            {
                buffer.Enqueue(message);
            }

            foreach (var handler in handlers)
            {
                handler.Handle(message);
                handler.HandleAsync(message).Forget();
                handler.HandleAsync(message, default).Forget();
            }
        }

        public async UniTask PublishAsync(T message, bool buffered = false, CancellationToken token = default)
        {
            await PublishAsync(message, options.DefaultPublishAsyncStrategy, buffered, token);
        }

        public async UniTask PublishAsync(T message, PublishAsyncStrategy strategy, bool buffered = false, CancellationToken token = default)
        {
            if (buffered)
            {
                buffer.Enqueue(message);
            }

            if (handlers.Count <= 1 || strategy == PublishAsyncStrategy.Sequential)
            {
                foreach (var item in handlers)
                {
                    if (token == default) 
                        await item.HandleAsync(message);
                    else 
                        await item.HandleAsync(message, token);
                }
            }
            else
            {
                await new AsyncHandlerWhenAll<T>(handlers, message, token);
            }
        }

        public IDisposable Subscribe(IBrokerHandler<T> handler, bool handleBuffered = false, params BrokerHandlerFilter<T>[] filters)
        {
            if (handleBuffered)
            {
                while (buffer.Count > 0)
                {
                    handler.Handle(buffer.Dequeue());
                }
            }

            return InternalSubscribe(handler, filters);
        }

        public async UniTask<IDisposable> SubscribeAsync(IBrokerHandler<T> handler, bool handleBuffered = false, CancellationToken token = default, params BrokerHandlerFilter<T>[] filters)
        {
            if (handleBuffered)
            {
                while (buffer.Count > 0)
                {
                    var message = buffer.Dequeue();
                    if (token == default)
                        await handler.HandleAsync(message);
                    else
                        await handler.HandleAsync(message, token);
                }
            }

            return InternalSubscribe(handler, filters);
        }

        IDisposable InternalSubscribe(IBrokerHandler<T> handler, params BrokerHandlerFilter<T>[] filters)
        {
            lock (gate)
            {
                if (isDisposed)
                {
                    return options.HandlingSubscribeDisposedPolicy.Handle(nameof(EventBroker<T>));
                }

                handler = handlerFactory.BuildHandler(handler, filters);
                handlers.Add(handler);
                var subscription = new Subscription(this, handler);
                diagnosticsInfo.IncrementSubscribe(this, subscription);
                return subscription;
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                // Dispose is called when scope is finished.
                var count = handlers.Count;
                handlers.Clear();
                if (!isDisposed)
                {
                    isDisposed = true;
                    diagnosticsInfo.RemoveTargetDiagnostics(this, count);
                }
            }
        }


        sealed class Subscription : IDisposable
        {
            bool isDisposed;
            EventBroker<T> broker;
            IBrokerHandler<T> subscriptionKey;

            public Subscription(EventBroker<T> _broker, IBrokerHandler<T> _subscriptionKey)
            {
                broker = _broker;
                subscriptionKey = _subscriptionKey;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    lock (broker.gate)
                    {
                        broker.handlers.Remove(subscriptionKey);
                        broker.diagnosticsInfo.DecrementSubscribe(broker, this);
                    }
                }
            }
        }
    }
}