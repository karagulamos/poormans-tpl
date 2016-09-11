using System;
using System.Threading;

namespace PoormansTPL
{
    internal class PoormansAwaiter
    {
        private volatile bool _signaled;
        private readonly object _internalLocker = new object();

        private static readonly Lazy<PoormansAwaiter> LazyInstance = new Lazy<PoormansAwaiter>(() => new PoormansAwaiter(), true);

        private PoormansAwaiter() {}

        public static PoormansAwaiter GetAwaiter()
        {
            return LazyInstance.Value;
        }

        public bool Wait()
        {
            bool status = false;

            lock (_internalLocker)
            {
                while (!_signaled)
                    status = Monitor.Wait(_internalLocker);
                _signaled = false;
            }

            return status;
        }

        public void Signal()
        {
            lock (_internalLocker)
            {
                _signaled = true;
                Monitor.PulseAll(_internalLocker);
            }
        }

        public void WaitAny()
        {
            lock (_internalLocker)
            {
                while (!_signaled)
                    Monitor.Wait(_internalLocker);
            }
        }
    }
}
