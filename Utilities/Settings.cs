namespace Utilities
{
    public class Settings
    {
        public Settings()
        {
            Cancel = new CancelEx();

            // Assign the default static object
            Default = this;
        }
        public bool TestNoModify { get; set; }
        public CancelEx Cancel { get; }
        public int FileRetryCount { get; set; }
        public int FileRetryWaitTime { get; set; }
        public bool WaitForCancelFileRetry()
        {
            return Cancel.WaitForCancel(FileRetryWaitTime * 1000);
        }


        public static Settings Default;
    }
}
