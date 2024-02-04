using UniEvent.Internal;

namespace UniEvent
{
    [Preserve]
    public sealed class EventFactory
    {
        Options options;
        DiagnosticsInfo diagnosticsInfo;
        HandlerFactory factory;

        [Preserve]
        public EventFactory(Options _options, DiagnosticsInfo _diagnosticsInfo, HandlerFactory _factory)
        {
            options = _options;
            diagnosticsInfo = _diagnosticsInfo;
            factory = _factory;
        }

        public IEventBroker<T> CreateEvent<T>()
        {
            var broker = new EventBroker<T>(options, factory, diagnosticsInfo);
            return broker;
        }

        public ITopicBroker<K, T> CreateTopic<K, T>()
        {
            var broker = new TopicBroker<K, T>(options, factory, diagnosticsInfo);
            return broker;
        }
        
        public IEventRequester<T, R> CreateEvent<T, R>()
        {
            var requester = new EventRequester<T, R>(options, factory, diagnosticsInfo);
            return requester;
        }

        public ITopicRequester<K, T, R> CreateTopic<K, T, R>()
        {
            var requester = new TopicRequester<K, T, R>(options, factory, diagnosticsInfo);
            return requester;
        }
    }
}