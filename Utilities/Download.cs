using System;
using System.IO;
using System.Net;
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
            // Get response
            WebRequest webRequest = CreateWebRequest(uri);
            WebResponse webResponse = webRequest.GetResponse();
            size = webResponse.ContentLength;
            if (webResponse.GetType() == typeof(HttpWebResponse))
            {
                HttpWebResponse httpResponse = (HttpWebResponse)webResponse;
                modifiedTime = httpResponse.LastModified;
            }
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod().Name))
        {
            return false;
        }

        return true;
    }

    public static bool DownloadFile(Uri uri, string fileName)
    {
        try
        {
            // Get response
            WebRequest webRequest = CreateWebRequest(uri);
            WebResponse webResponse = webRequest.GetResponse();

            // Write response to file
            Stream webStream = webResponse.GetResponseStream();
            using FileStream fileStream = File.OpenWrite(fileName);
            webStream.CopyTo(fileStream);
            fileStream.Close();
            webStream.Close();
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod().Name))
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
            // Get response
            WebRequest webRequest = CreateWebRequest(uri);
            WebResponse webResponse = webRequest.GetResponse();

            // Write response to text
            Stream webStream = webResponse.GetResponseStream();
            using StreamReader textStream = new(webStream);
            value = textStream.ReadToEnd();
            textStream.Close();
            webStream.Close();
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod().Name))
        {
            return false;
        }

        return true;
    }

    public static WebRequest CreateWebRequest(Uri uri)
    {
        // Open request
        WebRequest webRequest = WebRequest.Create(uri);
        webRequest.Timeout = Timeout;
        webRequest.Headers.Add(HttpRequestHeader.UserAgent, Assembly.GetExecutingAssembly().GetName().Name);
        if (webRequest.GetType() == typeof(HttpWebRequest))
        {
            HttpWebRequest httpRequest = (HttpWebRequest)webRequest;
            httpRequest.ReadWriteTimeout = webRequest.Timeout;
        }
        return webRequest;
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

    private const int Timeout = 30 * 1000;
}