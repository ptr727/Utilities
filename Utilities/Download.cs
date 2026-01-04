using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides HTTP download utilities for files and strings.
/// </summary>
public static class Download
{
    private static readonly Lazy<HttpClient> s_httpClient = new(CreateHttpClient);

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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
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

        try
        {
            using Stream httpStream = GetHttpClient().GetStreamAsync(uri).GetAwaiter().GetResult();
            using FileStream fileStream = File.OpenWrite(fileName);
            httpStream.CopyTo(fileStream);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
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

        try
        {
            await using Stream httpStream = await GetHttpClient()
                .GetStreamAsync(uri, cancellationToken)
                .ConfigureAwait(false);
            await using FileStream fileStream = File.OpenWrite(fileName);
            await httpStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return (false, string.Empty);
        }
    }

    /// <summary>
    /// Gets the shared HttpClient instance.
    /// </summary>
    /// <returns>The HttpClient instance.</returns>
    public static HttpClient GetHttpClient() => s_httpClient.Value;

    /// <summary>
    /// Creates a URI with optional username and password credentials.
    /// </summary>
    /// <param name="url">The base URL.</param>
    /// <param name="userName">The username (optional).</param>
    /// <param name="password">The password (optional).</param>
    /// <returns>The constructed URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
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

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(180) };

        Assembly assembly = Assembly.GetExecutingAssembly();
        string productName = assembly.GetName().Name ?? "InsaneGenius.Utilities";
        string productVersion = assembly.GetName().Version?.ToString() ?? "1.0.0";

        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(productName, productVersion)
        );

        return client;
    }
}
