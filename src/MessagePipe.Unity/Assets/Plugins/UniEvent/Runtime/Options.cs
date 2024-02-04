using System;
using System.Collections.Generic;
using UniEvent.Internal;

namespace UniEvent
{
    public enum PublishAsyncStrategy
    {
        Parallel,
        Sequential
    }

    public enum InstanceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public enum HandlingSubscribeDisposedPolicy
    {
        Ignore,
        Throw
    }

    internal static class HandlingSubscribeDisposedPolicyExtensions
    {
        public static IDisposable Handle(this HandlingSubscribeDisposedPolicy policy, string name)
        {
            if (policy == HandlingSubscribeDisposedPolicy.Throw)
            {
                throw new ObjectDisposedException(name);
            }

            return DisposableBag.Empty;
        }
    }

    public sealed class Options
    {
        /// <summary>AsyncPublisher.PublishAsync's concurrent strategy, default is Parallel.</summary>
        public PublishAsyncStrategy DefaultPublishAsyncStrategy { get; set; }

        /// <summary>For diagnostics usage, enable MessagePipeDiagnosticsInfo.CapturedStackTraces; default is false.</summary>
        public bool EnableCaptureStackTrace { get; set; }

        /// <summary>Choose how work on subscriber.Subscribe when after disposed, default is Ignore.</summary>
        public HandlingSubscribeDisposedPolicy HandlingSubscribeDisposedPolicy { get; set; }

        /// <summary>Default publisher/subscriber's lifetime scope, default is Singleton.</summary>
        public InstanceLifetime InstanceLifetime { get; set; }

        /// <summary>Default IRequestHandler/IAsyncRequestHandler's lifetime scope, default is Scoped.</summary>
        public InstanceLifetime RequestHandlerLifetime { get; set; }

        public Options()
        {
            DefaultPublishAsyncStrategy = PublishAsyncStrategy.Parallel;
            InstanceLifetime = InstanceLifetime.Singleton;
            RequestHandlerLifetime = InstanceLifetime.Scoped;
            EnableCaptureStackTrace = true;
            HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Ignore;
        }

        internal IEnumerable<Type> GetGlobalFilterTypes()
        {
            foreach (var item in brokerFilters)
            {
                yield return item.FilterType;
            }
        }

        List<FilterDef> brokerFilters = new List<FilterDef>();

        public void AddGlobalBrokerFilter<T>(int order = 0) where T : IBrokerHandlerFilter
        {
            brokerFilters.Add(new BrokerHandlerFilterDef(typeof(T), order, typeof(BrokerHandlerFilter<>)));
        }

        internal bool TryGetGlobalBrokerFilters<T>(IServiceProvider provider, out IEnumerable<IBrokerHandlerFilter> results)
        {
            if (brokerFilters.Count > 0)
            {
                results = CreateFilters<IBrokerHandlerFilter>(brokerFilters, provider, typeof(T));
                return true;
            }

            results = null;
            return false;
        }


        static IEnumerable<T> CreateFilters<T>(List<FilterDef> filterDefinitions, IServiceProvider provider, Type messageType) where T : IHandlerFilter
        {
            return CreateFiltersCore<T>(filterDefinitions, provider, messageType);
        }

        static IEnumerable<T> CreateFiltersCore<T>(List<FilterDef> filterDefinitions, IServiceProvider provider, Type messageType) where T : IHandlerFilter
        {
            foreach (var definition in filterDefinitions)
            {
                if (definition is BrokerHandlerFilterDef def)
                {
                    var filterType = def.FilterType;
                    if (def.IsOpenGenerics)
                    {
                        filterType = filterType.MakeGenericType(messageType);
                    }
                    else if (def.MessageType != messageType)
                    {
                        continue;
                    }

                    var filter = provider.GetService<T>(filterType);
                    filter.Order = def.Order;
                    yield return filter;
                }
            }
        }

        static IEnumerable<T> CreateFilters<T>(List<FilterDef> filterDefinitions, IServiceProvider provider, Type requestType, Type responseType) where T : IHandlerFilter
        {
            if (filterDefinitions.Count == 0) return Array.Empty<T>();
            return CreateFiltersCore<T>(filterDefinitions, provider, requestType, responseType);
        }

        static IEnumerable<T> CreateFiltersCore<T>(List<FilterDef> filterDefinitions, IServiceProvider provider, Type requestType, Type responseType) where T : IHandlerFilter
        {
            foreach (var definition in filterDefinitions)
            {
                if (definition is RequestFilterDef def)
                {
                    var filterType = def.FilterType;
                    if (def.IsOpenGenerics)
                    {
                        filterType = filterType.MakeGenericType(requestType, responseType);
                    }
                    else if (!(def.RequestType == requestType && def.ResponseType == responseType))
                    {
                        continue;
                    }

                    var filter = provider.GetService<T>(filterType);
                    filter.Order = def.Order;
                    yield return filter;
                }
            }
        }

        void ValidateFilterType(Type type, Type filterType)
        {
            if (!filterType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type.FullName} is not {filterType.Name}");
            }
        }
    }
}