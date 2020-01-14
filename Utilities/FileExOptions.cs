namespace InsaneGenius.Utilities
{
    public class FileExOptions
    {
        public FileExOptions()
        {
            Cancel = new Signal();
        }
        public bool TestNoModify { get; set; }
        public Signal Cancel { get; }
        public int FileRetryCount { get; set; } = 1;
        public int FileRetryWaitTime { get; set; } = 5;
        public bool WaitForCancelFileRetry()
        {
            return Cancel.WaitForSet(FileRetryWaitTime * 1000);
        }
    }
}
