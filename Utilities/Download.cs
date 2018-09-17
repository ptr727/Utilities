using System;
using System.IO;
using System.Net;

namespace InsaneGenius.Utilities
{
    public static class Download
    {
        public static bool GetContentInfo(string url, out long size, out DateTime modifiedtime)
        {
            size = 0;
            modifiedtime = DateTime.MinValue;
            try
            {
                // Get the file details
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "HEAD";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                size = resp.ContentLength;
                modifiedtime = resp.LastModified;
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
            try
            {
                // Open web request with timeout
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 15 * 1000;
                req.ReadWriteTimeout = req.Timeout;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream wstrm = resp.GetResponseStream();
                if (wstrm == null) throw new ArgumentNullException(nameof(wstrm));

                // Write response to file
                using (FileStream fstrm = File.OpenWrite(filename))
                {
                    wstrm.CopyTo(fstrm);
                    fstrm.Close();
                    wstrm.Close();
                }
            }
            catch (Exception e)
            {
                ConsoleEx.WriteLineError(e);
                return false;
            }
            return true;
        }

    }
}
