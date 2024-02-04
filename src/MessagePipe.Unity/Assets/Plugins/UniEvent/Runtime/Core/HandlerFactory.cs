using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniEvent.Internal;

namespace UniEvent
{
    [Preserve]
    public sealed class HandlerFactory
    {
        Options options;
        AttributeFilterProvider<BrokerFilterAttribute> filterProvider;
        IServiceProvider provider;

        [Preserve]
        public HandlerFactory(Options _options, AttributeFilterProvider<BrokerFilterAttribute> _filterProvider, IServiceProvider _provider)
        {
            options = _options;
            filterProvider = _filterProvider;
            provider = _provider;
        }

        public IBrokerHandler<T> BuildHandler<T>(IBrokerHandler<T> handler, BrokerHandlerFilter<T>[] filters)
        {
            var hasG = options.TryGetGlobalBrokerFilters<T>(provider, out var globalFilters);
            var hasH = filterProvider.TryGetAttributeFilters(handler.GetType(), provider, out var handlerFilters);
            if (filters.Length != 0 || hasG || hasH)
            {
                var brokerFilters = globalFilters.Concat(handlerFilters).Concat(filters).Cast<BrokerHandlerFilter<T>>();
                handler = new HandlerWrapper<T>(handler, brokerFilters);
            }

            return handler;
        }

        private sealed class HandlerWrapper<T> : IBrokerHandler<T>
        {
            Action<T> handler;
            Func<T, UniTask> handlerAsync;
            Func<T, CancellationToken, UniTask> handlerAsyncCancelable;

            public HandlerWrapper(IBrokerHandler<T> body, IEnumerable<BrokerHandlerFilter<T>> filters)
            {
                Action<T> next = body.Handle;
                Func<T, UniTask> nextAsync = body.HandleAsync;
                Func<T, CancellationToken, UniTask> nextAsyncCancelable = body.HandleAsync;

                foreach (var filter in filters.OrderByDescending(x => x.Order))
                {
                    next = new _Runner(filter, next).Handle;
                    nextAsync = new _Runner(filter, nextAsync).HandleAsync;
                    nextAsyncCancelable = new _Runner(filter, nextAsyncCancelable).HandleAsync;
                }

                handler = next;
                handlerAsync = nextAsync;
                handlerAsyncCancelable = nextAsyncCancelable;
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


            private sealed class _Runner
            {
                BrokerHandlerFilter<T> filter;
                Action<T> next;
                Func<T, UniTask> nextAsync;
                Func<T, CancellationToken, UniTask> nextAsyncCancelable;

                public _Runner(BrokerHandlerFilter<T> _filter, Action<T> _next)
                {
                    filter = _filter;
                    next = _next;
                }

                public _Runner(BrokerHandlerFilter<T> _filter, Func<T, UniTask> _next)
                {
                    filter = _filter;
                    nextAsync = _next;
                }

                public _Runner(BrokerHandlerFilter<T> _filter, Func<T, CancellationToken, UniTask> _next)
                {
                    filter = _filter;
                    nextAsyncCancelable = _next;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Handle(T message)
                {
                    if (next != null)
                    {
                        filter.Handle(message, next);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public UniTask HandleAsync(T message)
                {
                    if (nextAsync != null)
                    {
                        return filter.HandleAsync(message, nextAsync);
                    }

                    return default;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public UniTask HandleAsync(T message, CancellationToken token)
                {
                    if (nextAsyncCancelable != null)
                    {
                        return filter.HandleAsync(message, token, nextAsyncCancelable);
                    }

                    return default;
                }
            }
        }
    }
}