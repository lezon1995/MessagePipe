using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniEvent.Internal
{
    internal static class ContinuationSentinel
    {
        public static Action AvailableContinuation = () => { };
        public static Action CompletedContinuation = () => { };
    }

    internal partial class AsyncHandlerWhenAll<T> : ICriticalNotifyCompletion
    {
        int taskCount;
        int completedCount;
        ExceptionDispatchInfo exception;
        Action continuation = ContinuationSentinel.AvailableContinuation;

        public AsyncHandlerWhenAll(List<IBrokerHandler<T>> handlers, T message, CancellationToken token)
        {
            taskCount = handlers.Count;

            foreach (var handler in handlers)
            {
                try
                {
                    UniTask.Awaiter awaiter;
                    if (token == default)
                        awaiter = handler.HandleAsync(message).GetAwaiter();
                    else
                        awaiter = handler.HandleAsync(message, token).GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        awaiter.GetResult();
                        goto SUCCESSFULLY;
                    }

                    AwaiterNode.RegisterUnsafeOnCompleted(this, awaiter);
                    continue;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryInvokeContinuation();
                    return;
                }

                SUCCESSFULLY:
                IncrementSuccessfully();
            }
        }

        void IncrementSuccessfully()
        {
            if (Interlocked.Increment(ref completedCount) == taskCount)
            {
                TryInvokeContinuation();
            }
        }

        void TryInvokeContinuation()
        {
            var c = Interlocked.Exchange(ref continuation, ContinuationSentinel.CompletedContinuation); // register completed.
            if (c != ContinuationSentinel.AvailableContinuation && c != ContinuationSentinel.CompletedContinuation)
            {
                c();
            }
        }

        // Awaiter

        public AsyncHandlerWhenAll<T> GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted => exception != null || completedCount == taskCount;

        public void GetResult()
        {
            exception?.Throw();
        }

        public void OnCompleted(Action _continuation)
        {
            UnsafeOnCompleted(_continuation);
        }

        public void UnsafeOnCompleted(Action _continuation)
        {
            var c = Interlocked.CompareExchange(ref continuation, _continuation, ContinuationSentinel.AvailableContinuation);
            if (c == ContinuationSentinel.CompletedContinuation) // registered TryInvokeContinuation first.
            {
                _continuation();
            }
        }
    }


    internal partial class AsyncHandlerWhenAll<T, R> : ICriticalNotifyCompletion
    {
        int completedCount;
        ExceptionDispatchInfo exception;
        Action continuation = ContinuationSentinel.AvailableContinuation;
        R[] results;

        public AsyncHandlerWhenAll(List<IRequesterHandler<T, R>> handlers, T message, CancellationToken token)
        {
            results = new R[handlers.Count];

            for (var i = 0; i < handlers.Count; i++)
            {
                try
                {
                    var handler = handlers[i];
                    bool success;
                    UniTask<R> task;
                    if (token == default)
                        (success, task) = handler.TryHandleAsync(message);
                    else
                        (success, task) = handler.TryHandleAsync(message, token);

                    if (success)
                    {
                        var awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            results[i] = awaiter.GetResult();
                        }
                        else
                        {
                            AwaiterNode.RegisterUnsafeOnCompleted(this, awaiter, i);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryInvokeContinuation();
                    return;
                }

                IncrementSuccessfully();
            }
        }

        void IncrementSuccessfully()
        {
            if (Interlocked.Increment(ref completedCount) == results.Length)
            {
                TryInvokeContinuation();
            }
        }

        void TryInvokeContinuation()
        {
            var c = Interlocked.Exchange(ref continuation, ContinuationSentinel.CompletedContinuation); // register completed.
            if (c != ContinuationSentinel.AvailableContinuation && c != ContinuationSentinel.CompletedContinuation)
            {
                c();
            }
        }

        // Awaiter

        public AsyncHandlerWhenAll<T, R> GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return exception != null || completedCount == results.Length; }
        }

        public R[] GetResult()
        {
            exception?.Throw();
            return results;
        }

        public void OnCompleted(Action _continuation)
        {
            UnsafeOnCompleted(_continuation);
        }

        public void UnsafeOnCompleted(Action _continuation)
        {
            var c = Interlocked.CompareExchange(ref continuation, _continuation, ContinuationSentinel.AvailableContinuation);
            if (c == ContinuationSentinel.CompletedContinuation) // registered TryInvokeContinuation first.
            {
                _continuation();
            }
        }
    }
}