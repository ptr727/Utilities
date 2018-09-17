using System.Threading;

namespace InsaneGenius.Utilities
{
    public class CancelEx
    {
        public CancelEx()
        {
            _cancelevent = new ManualResetEvent(false);
        }

        public bool Cancel
        {
            get => _cancelevent.WaitOne(0);
            set
            {
                // Signal or reset the event
                if (value)
                    _cancelevent.Set();
                else
                    _cancelevent.Reset();
            }
        }

        public bool WaitForCancel(int milliseconds)
        {
            // Wait for event to be signalled
            return _cancelevent.WaitOne(milliseconds);
        }

        private ManualResetEvent _cancelevent;
    }
}
