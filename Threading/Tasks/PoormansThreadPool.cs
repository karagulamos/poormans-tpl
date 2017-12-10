using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PoormansTPL.Threading.Tasks
{
    public class PoormansThreadPool
    {
        private readonly ConcurrentQueue<Action> _workQueue;
        private readonly ConcurrentDictionary<PoormansWorkerThread, bool> _workerThreads;

        public PoormansThreadPool() : this(0, Environment.ProcessorCount)
        { }

        public void EnqueueTask(Action task)
        {
            _workQueue.Enqueue(task);

            var foundIdleWorkerToPerformTask = false;

            foreach (var workerThread in _workerThreads.Keys)
            {
                if (workerThread.IsBusy) continue;
                workerThread.WakeUp();
                foundIdleWorkerToPerformTask = true;
                break;
            }

            if (foundIdleWorkerToPerformTask || _workerThreads.Count >= MaximumWorkersAllowed)
                return;

            _workerThreads.TryAdd(new PoormansWorkerThread(_workQueue), true);
        }

        private void ManageWorkerThreads()
        {
            var idleTime = TimeSpan.FromSeconds(1);

            while (_managementThreadCanRun)
            {
                if (_workerThreads.Count <= MinimumWorkersAllowed)
                    continue;

                foreach (var workerThread in _workerThreads.Keys)
                {
                    TimeSpan lastExecutionTime = DateTime.Now.Subtract(workerThread.LastRun);

                    if (lastExecutionTime <= MaximumIdleTime)
                        continue;

                    workerThread.Shutdown();

                    bool result;
                    _workerThreads.TryRemove(workerThread, out result);
                }

                _managementThread.Join(idleTime);
            }
        }

        public void Shutdown()
        {
            _managementThreadCanRun = false;

            if (_managementThread == null) return;

            if (_managementThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                _managementThread.Interrupt();
            }

            _managementThread.Join();
            _managementThread = null;

            foreach (var workerThread in _workerThreads.Keys)
            {
                workerThread.Shutdown();
            }

            _workerThreads.Clear();
        }

        ~PoormansThreadPool()
        {
            Shutdown();
        }

        private int MinimumWorkersAllowed { get; }
        private int MaximumWorkersAllowed { get; }
        private TimeSpan MaximumIdleTime { get; } = TimeSpan.FromSeconds(5);

        private Thread _managementThread;
        private bool _managementThreadCanRun;

        public PoormansThreadPool(int minimumThreads, int maximumThreads, TimeSpan maximumIdleTimeBeforeRecycle = default(TimeSpan))
        {
            _workQueue = new ConcurrentQueue<Action>();
            _workerThreads = new ConcurrentDictionary<PoormansWorkerThread, bool>();

            MinimumWorkersAllowed = minimumThreads;
            MaximumWorkersAllowed = maximumThreads;
            MaximumIdleTime = maximumIdleTimeBeforeRecycle == default(TimeSpan) ? MaximumIdleTime : maximumIdleTimeBeforeRecycle;

            _managementThread = new Thread(ManageWorkerThreads)
            {
                IsBackground = true
            };
            _managementThreadCanRun = true;
            _managementThread.Start();
        }
    }
}
