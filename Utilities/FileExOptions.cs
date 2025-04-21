using System.Threading;

namespace InsaneGenius.Utilities;

public class FileExOptions
{
    // Test only, do not execute changes
    public bool TestNoModify { get; set; }

    // Cancel waiting operations
    public CancellationToken Cancel { get; set; } = CancellationToken.None;

    // Number of retries
    public int RetryCount { get; set; } = 1;

    // Wait time between retries in seconds
    public int RetryWaitTime { get; set; } = 5;

    public bool RetryWaitForCancel() => Cancel.WaitHandle.WaitOne(RetryWaitTime * 1000);
}
