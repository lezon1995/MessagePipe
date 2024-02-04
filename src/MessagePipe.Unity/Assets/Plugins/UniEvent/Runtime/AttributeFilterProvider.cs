#if !UNITY_2018_3_OR_NEWER
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UniEvent.Internal;

namespace UniEvent
{
    // not intended to use directly, use FilterAttachedMessageHandlerFactory.
    [Preserve]
    public sealed class AttributeFilterProvider<TAttribute> where TAttribute : IFilterAttribute
    {
        // cache attribute defines.
        ConcurrentDictionary<Type, AttributeFilterDef[]> cache;

        [Preserve]
        public AttributeFilterProvider()
        {
            cache = new ConcurrentDictionary<Type, AttributeFilterDef[]>();
        }

        public bool TryGetAttributeFilters(Type handlerType, IServiceProvider provider, out IEnumerable<IHandlerFilter> filters)
        {
            filters = null;
            if (cache.TryGetValue(handlerType, out var value))
            {
                if (value.Length == 0)
                {
                    return false;
                }

                filters = CreateFilters(value, provider);
                return true;
            }

            // require to get all filter for validate.
            var filterAttributes = handlerType.GetCustomAttributes(typeof(IFilterAttribute), true).OfType<TAttribute>().ToArray();
            if (filterAttributes.Length == 0)
            {
                cache[handlerType] = Array.Empty<AttributeFilterDef>();
                return false;
            }

            var array = filterAttributes.Select(x => new AttributeFilterDef(x.Type, x.Order)).ToArray();
            var filterDefinitions = cache.GetOrAdd(handlerType, array);
            filters = CreateFilters(filterDefinitions, provider);
            return true;
        }

        static IEnumerable<IHandlerFilter> CreateFilters(AttributeFilterDef[] filterDefinitions, IServiceProvider provider)
        {
            foreach (var filterDefinition in filterDefinitions)
            {
                var f = provider.GetService<IHandlerFilter>(filterDefinition.FilterType);
                f.Order = filterDefinition.Order;
                yield return f;
            }
        }
    }
}