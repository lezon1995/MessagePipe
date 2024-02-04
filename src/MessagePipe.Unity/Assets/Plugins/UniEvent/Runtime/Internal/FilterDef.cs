using System;
using System.Linq;

namespace UniEvent.Internal
{
    internal abstract class FilterDef
    {
        public Type FilterType { get; }
        public int Order { get; }

        protected FilterDef(Type filterType, int order)
        {
            FilterType = filterType;
            Order = order;
        }
    }

    internal sealed class AttributeFilterDef : FilterDef
    {
        public AttributeFilterDef(Type filterType, int order) : base(filterType, order)
        {
        }
    }

    internal sealed class BrokerHandlerFilterDef : FilterDef
    {
        public Type MessageType { get; }
        public bool IsOpenGenerics { get; }

        public BrokerHandlerFilterDef(Type filterType, int order, Type interfaceGenericDefinition) : base(filterType, order)
        {
            if (filterType.IsGenericType && !filterType.IsConstructedGenericType)
            {
                IsOpenGenerics = true;
                MessageType = null;
            }
            else
            {
                var genericDefinition = interfaceGenericDefinition;
                IsOpenGenerics = false;
                var interfaceType = filterType.GetBaseTypes()
                    .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericDefinition);

                var genArgs = interfaceType.GetGenericArguments();
                MessageType = genArgs[0];
            }
        }
    }

    internal sealed class RequestFilterDef : FilterDef
    {
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public bool IsOpenGenerics { get; }

        public RequestFilterDef(Type filterType, int order, Type interfaceGenericDefinition) : base(filterType, order)
        {
            if (filterType.IsGenericType && !filterType.IsConstructedGenericType)
            {
                IsOpenGenerics = true;
                RequestType = null;
                ResponseType = null;
            }
            else
            {
                IsOpenGenerics = false;
                var interfaceType = filterType.GetBaseTypes().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceGenericDefinition);

                var genArgs = interfaceType.GetGenericArguments();
                RequestType = genArgs[0];
                ResponseType = genArgs[1];
            }
        }
    }
}