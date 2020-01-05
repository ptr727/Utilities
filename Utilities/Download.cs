using System;
using System.IO;
using System.Net;

namespace InsaneGenius.Utilities
{
    public static class Download
    {
        public static bool GetContentInfo(string url, out long size, out DateTime modifiedTime)
        {
            size = 0;
            modifiedTime = DateTime.MinValue;
            try
            {
                // Get the file details
                WebRequest request = WebRequest.Create(url);
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
                ConsoleEx.WriteLineError(e);
                return false;
            }
            return true;
        }

        public static bool DownloadFile(string url, string fileName)
        {
            return DownloadFile(url, null, null, fileName);
        }

        public static bool DownloadFile(string url, string userName, string password, string fileName)
        {
            try
            {
                // Open request with timeout and credentials
                WebRequest request = WebRequest.Create(url);
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
                if (webStream == null) throw new ArgumentNullException(nameof(webStream));

                // Write response to file
                using FileStream fileStream = File.OpenWrite(fileName);
                webStream.CopyTo(fileStream);
                fileStream.Close();
                webStream.Close();
            }
            catch (Exception e)
            {
                ConsoleEx.WriteLineError(e);
                return false;
            }
            return true;
        }

        private const int Timeout = 30 * 1000;
    }
}
