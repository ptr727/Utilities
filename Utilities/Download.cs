using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

// TODO : Consider switching to HttpClient
// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netcore-3.1
// https://stackoverflow.com/questions/122853/how-to-get-the-file-size-from-http-headers

namespace InsaneGenius.Utilities;

public static class Download
{
    public static bool GetContentInfo(Uri uri, out long size, out DateTime modifiedTime)
    {
        size = 0;
        modifiedTime = DateTime.MinValue;
        try
        {
            // Send GET to URL
            using var httpResponse = GetHttpClient().GetAsync(uri).Result;
            httpResponse.EnsureSuccessStatusCode();

            // Get response
            size = (long)httpResponse.Content.Headers.ContentLength;
            modifiedTime = (DateTime)httpResponse.Content.Headers.LastModified?.DateTime;
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    public static bool DownloadFile(Uri uri, string fileName)
    {
        try
        {
            // Get HTTP stream
            var httpStream = GetHttpClient().GetStreamAsync(uri).Result;

            // Get file stream
            using FileStream fileStream = File.OpenWrite(fileName);

            // Write HTTP to file
            httpStream.CopyTo(fileStream);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    public static bool DownloadString(Uri uri, out string value)
    {
        value = null;
        try
        {
            value = GetHttpClient().GetStringAsync(uri).Result;
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    public static HttpClient GetHttpClient()
    {
        // TODO: How does static init and synchronization work?
        if (!HttpInit)
        {
            // Default timeout
            HttpClient.Timeout = TimeSpan.FromSeconds(30);

            // Some services only work if a user agent is set, use the assembly info
            // TODO: Set only once, not sure if Add() will keep adding the same value multiple times?
            HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Assembly.GetExecutingAssembly().GetName().Name,
                Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            HttpInit = true;
        }

        // Reuse the HTTP client per best practices
        // Not able to "easily" use IHttpClientFactory
        return HttpClient;
    }

    public static Uri CreateUri(string url, string userName, string password)
    {
        // Create Uri
        UriBuilder uriBuilder = new(url);
        if (!string.IsNullOrEmpty(userName))
            uriBuilder.UserName = userName;
        if (!string.IsNullOrEmpty(password))
            uriBuilder.Password = password;
        return uriBuilder.Uri;
    }

    private static bool HttpInit;
    private static readonly HttpClient HttpClient = new();
}