#if !UNITY_2018_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniEvent.Internal
{
    internal static class TypeCollector
    {
        public static IEnumerable<Type> CollectFromCurrentDomain()
        {
            var wellKnownIgnoreAssemblies = new[]
            {
                "netstandard",
                "System.*",
                "Microsoft.Win32.*",
                "Microsoft.Extensions.*",
                "Microsoft.AspNetCore",
                "Microsoft.AspNetCore.*",
                "Grpc.*",
                "MessagePack",
                "MessagePack.*",
                "MagicOnion.Server",
                "MagicOnion.Server.*",
                "MagicOnion.Client",
                "MagicOnion.Client.*",
                "MagicOnion.Abstractions",
                "MagicOnion.Shared",
            };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetName().Name != "UniEvent" && x.GetName().Name != "UniEvent.Redis")
                .Where(x =>
                {
                    return !wellKnownIgnoreAssemblies.Any(y =>
                    {
                        if (y.EndsWith("*"))
                        {
                            return x.GetName().Name!.StartsWith(y.Substring(0, y.Length - 1));
                        }
                        else
                        {
                            return x.GetName().Name == y;
                        }
                    });
                });

            return CollectFromAssemblies(assemblies);
        }

        public static IEnumerable<Type> CollectFromAssemblies(IEnumerable<Assembly> searchAssemblies)
        {
            var types = searchAssemblies
                .Where(x => x.GetName().Name != "UniEvent" && x.GetName().Name != "UniEvent.Redis")
                .SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                })
                .Where(x => x != null);

            return types!;
        }

        
    }
}

#endif