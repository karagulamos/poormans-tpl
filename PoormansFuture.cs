using System;

namespace PoormansTPL
{
    internal sealed class PoormansTask<TResult> : PoormansTask
    {
        private TResult _result;

        public PoormansTask(Func<TResult> action)
        {
            base.CreateNewThreadObject(() => _result = action());
        }
        
        public TResult Result
        {
            get
            {
                base.Wait();
                return _result;
            }
        }

        // The WaitAll & WaitAny implementations below are static shadows of the base implementation
        // used to suppress Resharper / IDE warnings around co-variant conversions between generic
        // and non-generic invocations of these methods in client code. Nevertheless, this works just
        // fine in our particular case.

        public static void WaitAll(params PoormansTask<TResult>[] parallelTasks)
        {
            PoormansTask.WaitAll(parallelTasks);
        }

        public static int WaitAny(params PoormansTask<TResult>[] parallelTasks)
        {
            return PoormansTask.WaitAny(parallelTasks);
        }

        public static int WaitAny(PoormansTask<TResult>[] parallelTasks, bool cancelRemainingTasks)
        {
            return PoormansTask.WaitAny(parallelTasks, cancelRemainingTasks);
        }

    }
}