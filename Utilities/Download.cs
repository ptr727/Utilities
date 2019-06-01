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
                    HttpWebResponse httpresponse = (HttpWebResponse)response;
                    modifiedTime = httpresponse.LastModified;
                }
            }
            catch (Exception e)
            {
                ConsoleEx.WriteLineError(e);
                return false;
            }
            return true;
        }

        public static bool DownloadFile(string url, string filename)
        {
            return DownloadFile(url, null, null, filename);
        }

        public static bool DownloadFile(string url, string username, string password, string filename)
        {
            try
            {
                // Open request with timeout and credentials
                WebRequest request = WebRequest.Create(url);
                request.Timeout = Timeout;
                if (request.GetType() == typeof(HttpWebRequest))
                {
                    HttpWebRequest httprequest = (HttpWebRequest)request;
                    httprequest.ReadWriteTimeout = request.Timeout;
                }
                if (string.IsNullOrEmpty(username) == false || string.IsNullOrEmpty(password) == false)
                {
                    request.Credentials = new NetworkCredential(username, password);
                }

                // Get the response
                WebResponse response = request.GetResponse();
                Stream webstream = response.GetResponseStream();
                if (webstream == null) throw new ArgumentNullException(nameof(webstream));

                // Write response to file
                using (FileStream filestream = File.OpenWrite(filename))
                {
                    webstream.CopyTo(filestream);
                    filestream.Close();
                    webstream.Close();
                }
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
