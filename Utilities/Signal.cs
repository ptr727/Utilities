using System.Threading;

namespace InsaneGenius.Utilities
{
    public class Signal
    {
        public Signal()
        {
            WaitEvent = new ManualResetEvent(false);
        }

        public bool State
        {
            get => WaitEvent.WaitOne(0);
            set
            {
                // Signal or reset the event
                if (value)
                    WaitEvent.Set();
                else
                    WaitEvent.Reset();
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
            return WaitEvent.WaitOne(milliseconds);
        }

        private readonly ManualResetEvent WaitEvent;
    }
}
