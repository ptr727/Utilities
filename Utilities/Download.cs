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
            return DownloadFile(uri, null, null, fileName);
        }

        public static bool DownloadFile(Uri uri, string userName, string password, string fileName)
        {
            try
            {
                // Open request with timeout and credentials
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = Timeout;
                if (request.GetType() == typeof(HttpWebRequest))
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)request;
                    httpRequest.ReadWriteTimeout = request.Timeout;
                }
                if (string.IsNullOrEmpty(userName) == false || string.IsNullOrEmpty(password) == false)
                {
                    request.Credentials = new NetworkCredential(userName, password);
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

        private const int Timeout = 30 * 1000;
    }
}
