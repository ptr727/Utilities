namespace ptr727.Utilities;

/// <summary>
/// Provides HTTP download utilities for files and strings.
/// </summary>
public static class Download
{
    private const int TemporaryFileAttempts = 5;

    private static readonly Lazy<HttpClient> s_httpClient = new(() =>
        HttpClientFactory.CreateClient(
            new HttpClientOptions { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) }
        )
    );
    private static readonly Lazy<ILogger> s_logger = new(() =>
        LogOptions.CreateLogger(typeof(Download).FullName!)
    );

    private static ILogger Log => s_logger.Value;

    /// <summary>
    /// Gets or sets the HTTP client timeout in seconds. Default is 180 seconds.
    /// Changes to this property only take effect before the first HTTP request is made.
    /// </summary>
    public static int TimeoutSeconds { get; set; } = 180;

    /// <summary>
    /// Gets content information (size and last modified time) from a URI.
    /// </summary>
    /// <param name="uri">The URI to get content information from.</param>
    /// <param name="size">The content size in bytes.</param>
    /// <param name="modifiedTime">The last modified time.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is null.</exception>
    public static bool GetContentInfo(Uri uri, out long size, out DateTime modifiedTime)
    {
        ArgumentNullException.ThrowIfNull(uri);

        size = 0;
        modifiedTime = DateTime.MinValue;
        try
        {
            using HttpResponseMessage httpResponse = GetHttpClient()
                .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
                .GetAwaiter()
                .GetResult()
                .EnsureSuccessStatusCode();

            size = httpResponse.Content.Headers.ContentLength ?? 0;
            modifiedTime = httpResponse.Content.Headers.LastModified?.DateTime ?? DateTime.MinValue;
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets content information (size and last modified time) from a URI asynchronously.
    /// </summary>
    /// <param name="uri">The URI to get content information from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing success status, size, and modified time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is null.</exception>
    public static async Task<(bool Success, long Size, DateTime ModifiedTime)> GetContentInfoAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);

        try
        {
            using HttpResponseMessage httpResponse = await GetHttpClient()
                .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            _ = httpResponse.EnsureSuccessStatusCode();

            long size = httpResponse.Content.Headers.ContentLength ?? 0;
            DateTime modifiedTime =
                httpResponse.Content.Headers.LastModified?.DateTime ?? DateTime.MinValue;
            return (true, size, modifiedTime);
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            return (false, 0, DateTime.MinValue);
        }
    }

    /// <summary>
    /// Downloads a file from a URI to a local file.
    /// </summary>
    /// <param name="uri">The URI to download from.</param>
    /// <param name="fileName">The local file path to save to.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> or <paramref name="fileName"/> is null.</exception>
    public static bool DownloadFile(Uri uri, string fileName)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(fileName);

        // Named only once the file behind it exists, so a failure before that deletes nothing.
        string tempFileName = string.Empty;
        try
        {
            using (Stream httpStream = GetHttpClient().GetStreamAsync(uri).GetAwaiter().GetResult())
            using (FileStream fileStream = CreateTemporaryFile(fileName, out tempFileName))
            {
                httpStream.CopyTo(fileStream);
            }

            // The destination is replaced only once the whole body is on disk.
            // A download that fails partway therefore leaves what was already there untouched.
            File.Move(tempFileName, fileName, overwrite: true);
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            DeleteTemporary(tempFileName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Downloads a file from a URI to a local file asynchronously.
    /// </summary>
    /// <param name="uri">The URI to download from.</param>
    /// <param name="fileName">The local file path to save to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> or <paramref name="fileName"/> is null.</exception>
    public static async Task<bool> DownloadFileAsync(
        Uri uri,
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(fileName);

        // Named only once the file behind it exists, so a failure before that deletes nothing.
        string tempFileName = string.Empty;
        try
        {
            Stream httpStream = await GetHttpClient()
                .GetStreamAsync(uri, cancellationToken)
                .ConfigureAwait(false);
            await using (httpStream.ConfigureAwait(false))
            {
                FileStream fileStream = CreateTemporaryFile(fileName, out tempFileName);
                await using (fileStream.ConfigureAwait(false))
                {
                    await httpStream
                        .CopyToAsync(fileStream, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            // The destination is replaced only once the whole body is on disk.
            // A download that fails partway therefore leaves what was already there untouched.
            File.Move(tempFileName, fileName, overwrite: true);
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            DeleteTemporary(tempFileName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Downloads a string from a URI.
    /// </summary>
    /// <param name="uri">The URI to download from.</param>
    /// <param name="value">The downloaded string content.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is null.</exception>
    public static bool DownloadString(Uri uri, out string value)
    {
        ArgumentNullException.ThrowIfNull(uri);

        value = string.Empty;
        try
        {
            value = GetHttpClient().GetStringAsync(uri).GetAwaiter().GetResult();
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Downloads a string from a URI asynchronously.
    /// </summary>
    /// <param name="uri">The URI to download from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing success status and the downloaded string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is null.</exception>
    public static async Task<(bool Success, string Value)> DownloadStringAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);

        try
        {
            string value = await GetHttpClient()
                .GetStringAsync(uri, cancellationToken)
                .ConfigureAwait(false);
            return (true, value);
        }
        catch (Exception e) when (Log.LogAndHandle(e))
        {
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Gets the shared HttpClient instance.
    /// </summary>
    /// <returns>The HttpClient instance.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1024:Use properties where appropriate",
        Justification = "Exposed as a method so callers see they receive the shared, lazily-initialized HttpClient rather than a lightweight property value."
    )]
    public static HttpClient GetHttpClient() => s_httpClient.Value;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership of the stream transfers to the caller, which disposes it."
    )]
    private static FileStream CreateTemporaryFile(string fileName, out string tempFileName)
    {
        // The temporary file sits beside the destination, so the replacing move stays on one volume.
        string? directory = Path.GetDirectoryName(fileName);
        string parent = string.IsNullOrEmpty(directory) ? "." : directory;

        for (int attempt = 0; ; attempt++)
        {
            string candidate = Path.Combine(parent, Path.GetRandomFileName());
            try
            {
                // CreateNew rather than Create, so a name already taken is never overwritten.
                FileStream stream = new(
                    candidate,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None
                );

                // Assigned only here, so the caller deletes a path this call actually created.
                tempFileName = candidate;
                return stream;
            }
            catch (IOException) when (attempt < TemporaryFileAttempts && File.Exists(candidate))
            {
                // A random name collided, and another name is the whole remedy.
                // Any other IO failure, a missing directory among them, is the caller's to report.
            }
        }
    }

    private static void DeleteTemporary(string fileName)
    {
        // Empty means no temporary file was created, so there is nothing of this call's to remove.
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            File.Delete(fileName);
        }
        catch (IOException)
        {
            // A partial download left behind is not itself a download failure.
        }
        catch (UnauthorizedAccessException)
        {
            // Same, where the file or its directory denies the delete.
        }
    }

    /// <summary>
    /// Creates a URI with optional username and password credentials.
    /// </summary>
    /// <param name="url">The base URL.</param>
    /// <param name="userName">The username (optional).</param>
    /// <param name="password">The password (optional).</param>
    /// <returns>The constructed URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1054:URI parameters should not be strings",
        Justification = "The url parameter is intentionally a string; CreateUri builds the Uri from raw string input supplied by callers."
    )]
    public static Uri CreateUri(string url, string? userName = null, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        UriBuilder uriBuilder = new(url);
        if (!string.IsNullOrEmpty(userName))
        {
            uriBuilder.UserName = userName;
        }

        if (!string.IsNullOrEmpty(password))
        {
            uriBuilder.Password = password;
        }

        return uriBuilder.Uri;
    }
}
