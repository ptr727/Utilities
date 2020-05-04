using System;
using InsaneGenius.Utilities;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri(@"https://api.github.com/repos/handbrake/handbrake/releases/latest");
            //Download.DownloadFile(uri, @"D:\Temp\foo.txt");;
            //Download.DownloadString(uri, out string value);

            uri = new Uri(@"https://github.com/HandBrake/HandBrake/releases/download/1.3.2/HandBrakeCLI-1.3.2-win-x86_64.zip");
            Download.GetContentInfo(uri, out long size, out DateTime time);
            Download.DownloadFile(uri, @"D:\Temp\foo.tmp");;
        }
    }
}
