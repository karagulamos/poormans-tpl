using System;
using System.Threading;

namespace PoormansTPL
{
    internal class PoormansSynchronization
    {
        private volatile bool _signaled;
        private readonly object _internalLocker = new object();

        private static readonly Lazy<PoormansSynchronization> LazyInstance = new Lazy<PoormansSynchronization>(() => new PoormansSynchronization(), true);

        public static PoormansSynchronization GetContext()
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
