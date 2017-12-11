﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PoormansTPL.Threading.Tasks
{
    public class PoormansThreadPool
    {
        private readonly ConcurrentQueue<Action> _workQueue;
        private readonly ConcurrentDictionary<PoormansWorkerThread, bool> _workerThreads;

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
            var idleTime = MaximumIdleTime;

            while (_managerThreadCanRun)
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

                try
                {
                    Thread.Sleep(idleTime);
                }
                catch { /* ignore */ }
            }
        }

        public void Shutdown()
        {
            _managerThreadCanRun = false;

            if (_managerThread == null) return;

            if (_managerThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                _managerThread.Interrupt();
            }

            _managerThread.Join();
            _managerThread = null;

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

        private Thread _managerThread;
        private bool _managerThreadCanRun;

        public PoormansThreadPool(int minimumThreads, int maximumThreads, TimeSpan maximumIdleTimeBeforeRecycle)
        {
            _workQueue = new ConcurrentQueue<Action>();
            _workerThreads = new ConcurrentDictionary<PoormansWorkerThread, bool>();

            MinimumWorkersAllowed = minimumThreads;
            MaximumWorkersAllowed = maximumThreads;
            MaximumIdleTime = maximumIdleTimeBeforeRecycle == default(TimeSpan) ? MaximumIdleTime : maximumIdleTimeBeforeRecycle;

            _managerThread = new Thread(ManageWorkerThreads)
            {
                IsBackground = true
            };
            _managerThreadCanRun = true;
            _managerThread.Start();
        }

        public PoormansThreadPool() : this(Environment.ProcessorCount)
        { }

        public PoormansThreadPool(int minimumThreads)
        : this(minimumThreads, Environment.ProcessorCount * 100)
        { }

        public PoormansThreadPool(int minimumThreads, int maximumThreads)
        : this(minimumThreads, maximumThreads, default(TimeSpan))
        { }
    }
}
