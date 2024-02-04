using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniEvent.Internal;

namespace UniEvent
{
    public static partial class SubscriberExtensions
    {
        #region Event Broker

        // pub/sub-keyless-sync

        public static IDisposable Subscribe<T>(this IEventBroker<T> subscriber, Action<T> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(new AnonymousHandler<T>(handler), false, filters);
        }

        public static IDisposable Subscribe<T>(this IEventBroker<T> subscriber, Action<T> handler, Func<T, bool> predicate, params BrokerHandlerFilter<T>[] filters)
        {
            var predicateFilter = new PredicateFilter<T>(predicate);
            filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(new AnonymousHandler<T>(handler), false, filters);
        }

        // pub/sub-keyless-async

        public static IDisposable Subscribe<T>(this IEventBroker<T> subscriber, Func<T, UniTask> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(new AnonymousHandler<T>(handler), false, filters);
        }

        public static IDisposable Subscribe<T>(this IEventBroker<T> subscriber, Func<T, CancellationToken, UniTask> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(new AnonymousHandler<T>(handler), false, filters);
        }

        public static IDisposable Subscribe<T>(this IEventBroker<T> subscriber, Func<T, CancellationToken, UniTask> handler, Func<T, bool> predicate, params BrokerHandlerFilter<T>[] filters)
        {
            var predicateFilter = new PredicateFilter<T>(predicate);
            filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(new AnonymousHandler<T>(handler), false, filters);
        }

        #endregion

        #region Topic Broker

        // pub/sub-key-sync

        public static IDisposable Subscribe<K, T>(this ITopicBroker<K, T> subscriber, K key, Action<T> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T>(handler), filters);
        }

        public static IDisposable Subscribe<K, T>(this ITopicBroker<K, T> subscriber, K key, Action<T> handler, Func<T, bool> predicate, params BrokerHandlerFilter<T>[] filters)
        {
            var predicateFilter = new PredicateFilter<T>(predicate);
            filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(key, new AnonymousHandler<T>(handler), filters);
        }

        // pub/sub-key-async

        public static IDisposable Subscribe<K, T>(this ITopicBroker<K, T> subscriber, K key, Func<T, UniTask> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T>(handler), filters);
        }

        public static IDisposable Subscribe<K, T>(this ITopicBroker<K, T> subscriber, K key, Func<T, CancellationToken, UniTask> handler, params BrokerHandlerFilter<T>[] filters)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T>(handler), filters);
        }

        public static IDisposable Subscribe<K, T>(this ITopicBroker<K, T> subscriber, K key, Func<T, CancellationToken, UniTask> handler, Func<T, bool> predicate, params BrokerHandlerFilter<T>[] filters)
        {
            var predicateFilter = new PredicateFilter<T>(predicate);
            filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(key, new AnonymousHandler<T>(handler), filters);
        }

        #endregion
    }

    public static partial class SubscriberExtensions
    {
        #region Event Requester

        // pub/sub-keyless-sync

        public static IDisposable Subscribe<T, R>(this IEventRequester<T, R> subscriber, Func<T, R> handler)
        {
            return subscriber.Subscribe(new AnonymousHandler<T, R>(handler));
        }

        public static IDisposable Subscribe<T, R>(this IEventRequester<T, R> subscriber, Func<T, R> handler, Func<T, bool> predicate)
        {
            // var predicateFilter = new PredicateFilter<T>(predicate);
            // filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(new AnonymousHandler<T, R>(handler) /*, false, filters*/);
        }

        // pub/sub-keyless-async

        public static IDisposable Subscribe<T, R>(this IEventRequester<T, R> subscriber, Func<T, UniTask<R>> handler)
        {
            return subscriber.Subscribe(new AnonymousHandler<T, R>(handler) /*, false, filters*/);
        }

        public static IDisposable Subscribe<T, R>(this IEventRequester<T, R> subscriber, Func<T, CancellationToken, UniTask<R>> handler)
        {
            return subscriber.Subscribe(new AnonymousHandler<T, R>(handler) /*, false, filters*/);
        }

        public static IDisposable Subscribe<T, R>(this IEventRequester<T, R> subscriber, Func<T, CancellationToken, UniTask<R>> handler, Func<T, bool> predicate)
        {
            // var predicateFilter = new PredicateFilter<T>(predicate);
            // filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(new AnonymousHandler<T, R>(handler) /*, false, filters*/);
        }

        #endregion

        #region Topic Requester

        // pub/sub-key-sync

        public static IDisposable Subscribe<K, T, R>(this ITopicRequester<K, T, R> subscriber, K key, Func<T, R> handler)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T, R>(handler) /*, filters*/);
        }

        public static IDisposable Subscribe<K, T, R>(this ITopicRequester<K, T, R> subscriber, K key, Func<T, R> handler, Func<T, bool> predicate)
        {
            // var predicateFilter = new PredicateFilter<T>(predicate);
            // filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(key, new AnonymousHandler<T, R>(handler) /*, filters*/);
        }

        // pub/sub-key-async

        public static IDisposable Subscribe<K, T, R>(this ITopicRequester<K, T, R> subscriber, K key, Func<T, UniTask<R>> handler)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T, R>(handler) /*, filters*/);
        }

        public static IDisposable Subscribe<K, T, R>(this ITopicRequester<K, T, R> subscriber, K key, Func<T, CancellationToken, UniTask<R>> handler)
        {
            return subscriber.Subscribe(key, new AnonymousHandler<T, R>(handler) /*, filters*/);
        }

        public static IDisposable Subscribe<K, T, R>(this ITopicRequester<K, T, R> subscriber, K key, Func<T, CancellationToken, UniTask<R>> handler, Func<T, bool> predicate)
        {
            // var predicateFilter = new PredicateFilter<T>(predicate);
            // filters = filters.Length == 0 ? new[] { predicateFilter } : ArrayUtil.ImmutableAdd(filters, predicateFilter);
            return subscriber.Subscribe(key, new AnonymousHandler<T, R>(handler) /*, filters*/);
        }

        #endregion
    }

    internal sealed class AnonymousHandler<T> : IBrokerHandler<T>
    {
        Action<T> handler;
        Func<T, UniTask> handlerAsync;
        Func<T, CancellationToken, UniTask> handlerAsyncCancelable;

        public AnonymousHandler(Action<T> _handler)
        {
            handler = _handler;
        }

        public AnonymousHandler(Func<T, UniTask> _handlerAsync)
        {
            handlerAsync = _handlerAsync;
        }

        public AnonymousHandler(Func<T, CancellationToken, UniTask> _handlerAsyncCancelable)
        {
            handlerAsyncCancelable = _handlerAsyncCancelable;
        }

        public void Handle(T message)
        {
            handler?.Invoke(message);
        }

        public UniTask HandleAsync(T message)
        {
            if (handlerAsync != null)
            {
                return handlerAsync.Invoke(message);
            }

            return default;
        }

        public UniTask HandleAsync(T message, CancellationToken token)
        {
            if (handlerAsyncCancelable != null)
            {
                return handlerAsyncCancelable.Invoke(message, token);
            }

            return default;
        }
    }

    internal sealed class AnonymousHandler<T, R> : IRequesterHandler<T, R>
    {
        Func<T, R> handler;
        Func<T, UniTask<R>> handlerAsync;
        Func<T, CancellationToken, UniTask<R>> handlerAsyncCancelable;

        public AnonymousHandler(Func<T, R> _handler)
        {
            handler = _handler;
        }

        public AnonymousHandler(Func<T, UniTask<R>> _handlerAsync)
        {
            handlerAsync = _handlerAsync;
        }

        public AnonymousHandler(Func<T, CancellationToken, UniTask<R>> _handlerAsyncCancelable)
        {
            handlerAsyncCancelable = _handlerAsyncCancelable;
        }

        public bool TryHandle(T message, out R result)
        {
            if (handler != null)
            {
                result = handler.Invoke(message);
                return true;
            }

            result = default;
            return false;
        }

        public (bool, UniTask<R>) TryHandleAsync(T message)
        {
            if (handlerAsync != null)
            {
                return (true, handlerAsync.Invoke(message));
            }

            return (false, default);
        }

        public (bool, UniTask<R>) TryHandleAsync(T message, CancellationToken token)
        {
            if (handlerAsyncCancelable != null)
            {
                return (true, handlerAsyncCancelable.Invoke(message, token));
            }

            return (false, default);
        }
    }
}