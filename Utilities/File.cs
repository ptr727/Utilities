using System;
using System.IO;
using System.Linq;

namespace Utilities
{
    public static class FileEx
    {
        // Delete file, and retry in case of failure
        public static bool DeleteFile(string filename)
        {
            // Test
            if (Settings.Default.TestNoModify)
                return true;

            bool result = false;
            for (int retrycount = 0; retrycount < Settings.Default.FileRetryCount; retrycount++)
            {
                // Break on cancel
                if (Settings.Default.Cancel.Cancel)
                    break;

                // Try to delete the file
                try
                {
                    ConsoleEx.WriteLine($"Deleting ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{filename}\"");
                    if (File.Exists(filename))
                        File.Delete(filename);
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    ConsoleEx.WriteLineError(e.Message);
                    Settings.Default.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    ConsoleEx.WriteLineError(e);
                    break;
                }
            }

            return result;
        }

        // Delete directory, and retry in case of failure
        public static bool DeleteDirectory(string directory)
        {
            // Test
            if (Settings.Default.TestNoModify)
                return true;

            bool result = false;
            for (int retrycount = 0; retrycount < Settings.Default.FileRetryCount; retrycount++)
            {
                // Break on cancel
                if (Settings.Default.Cancel.Cancel)
                    break;

                // Try to delete the directory
                try
                {
                    ConsoleEx.WriteLine($"Deleting ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{directory}\"");
                    if (Directory.Exists(directory))
                        Directory.Delete(directory);
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // TODO : Do not retry if folder is not empty, it will never succeed

                    // Retry
                    ConsoleEx.WriteLineError(e.Message);
                    Settings.Default.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    ConsoleEx.WriteLineError(e);
                    break;
                }
            }

            return result;
        }

        // Rename file, and retry in case of failure
        public static bool RenameFile(string originalname, string newname)
        {
            // Test
            if (Settings.Default.TestNoModify)
                return true;

            // Split path components so we can use them for pretty printing
            string originaldirectory = Path.GetDirectoryName(originalname);
            string originalfile = Path.GetFileName(originalname);
            string newdirectory = Path.GetDirectoryName(newname);
            string newfile = Path.GetFileName(newname);

            bool result = false;
            for (int retrycount = 0; retrycount < Settings.Default.FileRetryCount; retrycount++)
            {
                // Break on cancel
                if (Settings.Default.Cancel.Cancel)
                    break;

                // Try to rename the file
                // Delete the destination if it exists
                try
                {
                    ConsoleEx.WriteLine(originaldirectory.Equals(newdirectory, StringComparison.OrdinalIgnoreCase)
                        ? $"Renaming ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{originaldirectory}\" : \"{originalfile}\" to \"{newfile}\""
                        : $"Renaming ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{originalname}\" to \"{newname}\"");
                    if (File.Exists(newname))
                        File.Delete(newname);
                    File.Move(originalname, newname);
                    result = true;
                    break;
                }
                catch (FileNotFoundException e)
                {
                    // File not found
                    ConsoleEx.WriteLineError(e);
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    ConsoleEx.WriteLineError(e.Message);
                    Settings.Default.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    ConsoleEx.WriteLineError(e);
                    break;
                }
            }

            return result;
        }

        // Rename or move the folder, and retry in case of failure
        public static bool RenameFolder(string originalname, string newname)
        {
            // Test
            if (Settings.Default.TestNoModify)
                return true;

            bool result = false;
            for (int retrycount = 0; retrycount < Settings.Default.FileRetryCount; retrycount++)
            {
                // Break on cancel
                if (Settings.Default.Cancel.Cancel)
                    break;

                // Try to move the folder
                try
                {
                    ConsoleEx.WriteLine($"Renaming ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{originalname}\" to \"{newname}\"");
                    if (Directory.Exists(newname))
                        DeleteDirectory(newname, true);
                    Directory.Move(originalname, newname);
                    result = true;
                    break;
                }
                catch (FileNotFoundException e)
                {
                    // File not found
                    ConsoleEx.WriteLineError(e);
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    ConsoleEx.WriteLineError(e.Message);
                    Settings.Default.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    ConsoleEx.WriteLineError(e);
                    break;
                }
            }

            return result;
        }

        // Recursively delete empty directories in the directory
        public static bool DeleteEmptyDirectories(string directory, ref int deleted)
        {
            // Find all directories in this directory, not all subdirectories, we will call recursively
            DirectoryInfo parentinfo = new DirectoryInfo(directory);
            foreach (DirectoryInfo dirinfo in parentinfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                // Call recursively for this directory
                if (!DeleteEmptyDirectories(dirinfo.FullName, ref deleted))
                    return false;

                // Test for files and directories, if none, delete this directory
                if (dirinfo.GetFiles().Length != 0 || dirinfo.GetDirectories().Length != 0)
                    continue;

                if (!DeleteDirectory(dirinfo.FullName))
                    return false;
                deleted++;
            }

            return true;
        }

        // Recursively delete all the files and directories inside the directory
        public static bool DeleteInsideDirectory(string directory)
        {
            // Delete all files in this directory
            DirectoryInfo parentinfo = new DirectoryInfo(directory);
            if (parentinfo.GetFiles().Any(fileinfo => !DeleteFile(fileinfo.FullName)))
            {
                return false;
            }

            // Find all directories in this directory, not all subdirectories, we will call recursively
            foreach (DirectoryInfo dirinfo in parentinfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                // Call recursively for this directory
                if (!DeleteInsideDirectory(dirinfo.FullName))
                    return false;

                // Delete this directory
                if (!DeleteDirectory(dirinfo.FullName))
                    return false;
            }

            return true;
        }

        // Recursively delete the directory and files
        public static bool DeleteDirectory(string directory, bool recursive)
        {
            // Just delete the directory, will fail if not empty
            if (!recursive)
                return DeleteDirectory(directory);

            // Recursively delete inside the directory and the directory
            return DeleteInsideDirectory(directory) && DeleteDirectory(directory);
        }

        /*
                public static bool IsFileReadable(string filename)
                {
                    try
                    {
                        FileInfo fileinfo = new FileInfo(filename);
                        return IsFileReadable(fileinfo);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
        */

        public static bool WaitFileReadAble(string filename)
        {
            bool result = false;
            for (int retrycount = 0; retrycount < Settings.Default.FileRetryCount; retrycount++)
            {
                // Break on cancel
                if (Settings.Default.Cancel.Cancel)
                    break;

                // Try to access the file
                try
                {
                    ConsoleEx.WriteLine($"Waiting for file to become readable ({retrycount + 1} / {Settings.Default.FileRetryCount}) : \"{filename}\"");
                    FileInfo fileinfo = new FileInfo(filename);
                    FileStream stream = fileinfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                    stream.Close();
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    ConsoleEx.WriteLineError(e.Message);
                    Settings.Default.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    ConsoleEx.WriteLineError(e);
                    break;
                }
            }

            return result;
        }

        public static bool IsFileReadable(FileInfo fileinfo)
        {
            try
            {
                FileStream stream = fileinfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool AreFilesInfolderReadable(string foldername)
        {
            // Test each file for readability
            DirectoryInfo dirinfo = new DirectoryInfo(foldername);
            return dirinfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).All(IsFileReadable);
        }
    }
}
