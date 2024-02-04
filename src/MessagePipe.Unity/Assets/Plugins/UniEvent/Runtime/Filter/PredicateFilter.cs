using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniEvent
{
    internal sealed class PredicateFilter<T> : BrokerHandlerFilter<T>
    {
        Func<T, bool> predicate;

        public PredicateFilter(Func<T, bool> _predicate)
        {
            predicate = _predicate;
            Order = int.MinValue;
        }

        public override void Handle(T message, Action<T> next)
        {
            if (predicate(message))
            {
                next(message);
            }
        }

        public override UniTask HandleAsync(T message, Func<T, UniTask> next)
        {
            if (predicate(message))
            {
                return next(message);
            }

            return default;
        }

        public override UniTask HandleAsync(T message, CancellationToken token, Func<T, CancellationToken, UniTask> next)
        {
            if (predicate(message))
            {
                return next(message, token);
            }

            return default;
        }
    }
}