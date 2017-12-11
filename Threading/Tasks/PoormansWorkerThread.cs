using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PoormansTPL.Threading.Tasks
{
    internal class PoormansWorkerThread
    {
        private readonly ConcurrentQueue<Action> _taskQueue;
        private Thread _workerThread;
        private bool _workerThreadCanRun;

        internal PoormansWorkerThread(ConcurrentQueue<Action> taskQueue)
        {
            _taskQueue = taskQueue;

            _workerThread = new Thread(ManageEnqueuedTasks)
            {
                IsBackground = true
            };

            _workerThreadCanRun = true;
            _workerThread.Start();
        }

        public DateTime LastRun { get; private set; } = DateTime.Now;
        public bool IsBusy { get; private set; }

        private void ManageEnqueuedTasks()
        {
            var idleTime = TimeSpan.FromMilliseconds(100);

            while (_workerThreadCanRun)
            {
                try
                {
                    Action task;
                    while (_taskQueue.TryDequeue(out task))
                    {
                        LastRun = DateTime.Now;
                        task.Invoke();
                    }
                }
                catch { /* ignored */ }

                try
                {
                    IsBusy = false;
                    Thread.Sleep(idleTime);
                }
                catch { /* ignored */ }
            }
        }

        public void WakeUp()
        {
            if (_workerThread.ThreadState == ThreadState.WaitSleepJoin)
                _workerThread.Interrupt();

            IsBusy = true;
        }

        public void Shutdown()
        {
            if (_workerThread == null) return;

            _workerThreadCanRun = false;
            IsBusy = false;

            if (_workerThread.ThreadState == ThreadState.WaitSleepJoin)
                _workerThread.Interrupt();

            _workerThread.Join();
            _workerThread = null;
        }
    }
}
