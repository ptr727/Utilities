using System.Threading;

namespace InsaneGenius.Utilities;

/// <summary>
/// Configuration options for FileEx operations.
/// </summary>
public class FileExOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to run in test mode without making actual changes.
    /// </summary>
    public bool TestNoModify { get; set; }

    /// <summary>
    /// Gets or sets the cancellation token for canceling waiting operations.
    /// </summary>
    public CancellationToken Cancel { get; set; } = CancellationToken.None;

    /// <summary>
    /// Gets or sets the number of retry attempts for failed operations.
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the wait time between retries in seconds.
    /// </summary>
    public int RetryWaitTime { get; set; } = 5;

    /// <summary>
    /// Waits for the specified retry time or until cancellation is requested.
    /// </summary>
    /// <returns>True if cancellation was requested, false if the wait time elapsed.</returns>
    public bool RetryWaitForCancel() => Cancel.WaitHandle.WaitOne(RetryWaitTime * 1000);
}
