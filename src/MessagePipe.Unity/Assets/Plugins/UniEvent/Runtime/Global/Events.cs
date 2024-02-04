using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace UniEvent
{
    public static class Events
    {
        static IServiceProvider provider;
        static EventFactory factory;
        static DiagnosticsInfo diagnosticsInfo;
        public static bool IsInitialized => provider != null;
        public static DiagnosticsInfo DiagnosticsInfo => diagnosticsInfo;

        public static void SetProvider(IServiceProvider _provider)
        {
            provider = _provider;
            diagnosticsInfo = _provider.GetService<DiagnosticsInfo>();
            factory = _provider.GetService<EventFactory>();
        }

        internal static IEventBroker<T> CreateEvent<T>()
        {
            return factory.CreateEvent<T>();
        }

        internal static ITopicBroker<K, T> CreateTopic<K, T>()
        {
            return factory.CreateTopic<K, T>();
        }

        internal static IEventRequester<T, R> CreateEvent<T, R>()
        {
            return factory.CreateEvent<T, R>();
        }

        internal static ITopicRequester<K, T, R> CreateTopic<K, T, R>()
        {
            return factory.CreateTopic<K, T, R>();
        }

        #region Request

        // public static IRequest<Req, Res> GetRequest<Req, Res>()
        // {
        //     return provider.GetService<IRequest<Req, Res>>();
        // }
        //
        // public static IRequestAsync<Req, Res> GetAsyncRequest<Req, Res>()
        // {
        //     return provider.GetService<IRequestAsync<Req, Res>>();
        // }
        //
        // public static IRequestAll<Req, Res> GetRequestAll<Req, Res>()
        // {
        //     return provider.GetService<IRequestAll<Req, Res>>();
        // }
        //
        // public static IRequestAllAsync<Req, Res> GetAsyncRequestAll<Req, Res>()
        // {
        //     return provider.GetService<IRequestAllAsync<Req, Res>>();
        // }

        #endregion
    }
}