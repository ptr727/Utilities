using System.Threading;

namespace InsaneGenius.Utilities
{
    public class Signal
    {
        public Signal()
        {
            _event = new ManualResetEvent(false);
        }

        public bool State
        {
            get => _event.WaitOne(0);
            set
            {
                // Signal or reset the event
                if (value)
                    _event.Set();
                else
                    _event.Reset();
            }
        }

        public void Set()
        {
            State = true;
        }

        public void Reset()
        {
            State = false;
        }

        public bool WaitForSet(int milliseconds)
        {
            // Wait for event to be signaled
            return _event.WaitOne(milliseconds);
        }

        private readonly ManualResetEvent _event;
    }
}
