using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace InsaneGenius.Utilities
{
    public static class Download
    {
        public static bool GetContentInfo(Uri uri, out long size, out DateTime modifiedTime)
        {
            size = 0;
            modifiedTime = DateTime.MinValue;
            try
            {
                // Get the file details
                WebRequest request = WebRequest.Create(uri);
                request.Method = "HEAD";
                WebResponse response = request.GetResponse();
                size = response.ContentLength;
                if (response.GetType() == typeof(HttpWebResponse))
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    modifiedTime = httpResponse.LastModified;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
            return true;
        }

        public static bool DownloadFile(Uri uri, string fileName)
        {
            try
            {
                // Open request with timeout and credentials set in Uri
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = Timeout;
                if (request.GetType() == typeof(HttpWebRequest))
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)request;
                    httpRequest.ReadWriteTimeout = request.Timeout;
                }

                // Get the response
                WebResponse response = request.GetResponse();
                Stream webStream = response.GetResponseStream();

                // Write response to file
                using FileStream fileStream = File.OpenWrite(fileName);
                webStream.CopyTo(fileStream);
                fileStream.Close();
                webStream.Close();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
            return true;
        }

        public static bool DownloadFile(string url, string userName, string password, string fileName)
        {
            Uri uri;
            try
            {
                // Create Uri
                UriBuilder uriBuilder = new UriBuilder(url);
                if (!string.IsNullOrEmpty(userName))
                    uriBuilder.UserName = userName;
                if (!string.IsNullOrEmpty(password))
                    uriBuilder.Password = password;
                uri = uriBuilder.Uri;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
            return DownloadFile(uri, fileName);
        }

        private const int Timeout = 30 * 1000;
    }
}
