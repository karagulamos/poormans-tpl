using System;
using System.Collections.Generic;
using System.Threading;

namespace PoormansTPL
{
    internal class PoormansTask
    {
        private Thread _worker;
        private readonly List<Exception> _exceptions = new List<Exception>();
        private readonly object _exceptionSyncLocker = new object();
        private readonly PoormansAwaiter _awaiterContext = PoormansAwaiter.GetContext();

        protected PoormansTask() { }

        public PoormansTask(Action action)
        {
            this.CreateNewThreadObject(action);
        }

        public IReadOnlyList<Exception> Exceptions
        {
            get
            {
                lock (_exceptionSyncLocker)
                    return _exceptions;
            }
        }

        public void Start()
        {
            _worker.Start();
        }

        public static PoormansTask Run(Action action)
        {
            var poorTask = new PoormansTask(action);
            poorTask.Start();
            return poorTask;
        }

        public static PoormansTask<TResult> Run<TResult>(Func<TResult> action)
        {
            var poorTask = new PoormansTask<TResult>(action);
            poorTask.Start();
            return poorTask;
        }

        public static PoormansTask<TResult> Run<TResult>(Func<PoormansTask<TResult>> action)
        {
            var poorTask = new PoormansTask<PoormansTask<TResult>>(action);
            poorTask.Start();
            return poorTask.Result;
        }

        public static void WaitAll(params PoormansTask[] tasks)
        {
            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        public void Wait()
        {
            _worker.Join();
            this.ThrowAggregateExceptionIfFaulted();
        }
        public static int WaitAny(params PoormansTask[] tasks)
        {
            return WaitAny(tasks, false);
        }

        public static int WaitAny(PoormansTask[] tasks, bool cancelRemainingTasks)
        {
            var awaiter = PoormansAwaiter.GetContext();

            int completedTaskIndex = -1;

            while (completedTaskIndex < 0)
            {
                awaiter.WaitAny();
                completedTaskIndex = Array.FindIndex(tasks, t => t.HasCompleted());
            }

            if (cancelRemainingTasks)
            {
                foreach (var task in tasks)
                {
                    task.Cancel();
                }
            }

            return completedTaskIndex;
        }

        protected void Cancel()
        {
            _worker.Abort();
        }

        public bool HasCompleted()
        {
            return Thread.CurrentThread != _worker && _worker.Join(TimeSpan.Zero);
        }

        protected void ThrowAggregateExceptionIfFaulted()
        {
            lock (_exceptionSyncLocker)
            {
                if (_exceptions.Count > 0)
                    throw new AggregateException(_exceptions);
            }
        }

        protected void CreateNewThreadObject(Action action)
        {
            _worker = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    lock (_exceptionSyncLocker)
                        _exceptions.Add(exception);
                }
                finally
                {
                    _awaiterContext.Signal();
                }
            }) { IsBackground = true };

        }
    }
}