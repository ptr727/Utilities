using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace InsaneGenius.Utilities
{
    public static class FileEx
    {
        // Settings for file operations
        public static readonly FileExOptions Options = new FileExOptions();

        // Delete file, and retry in case of failure
        public static bool DeleteFile(string fileName)
        {
            // Test
            if (Options.TestNoModify)
                return true;

            bool result = false;
            for (int retryCount = 0; retryCount < Options.FileRetryCount; retryCount ++)
            {
                // Break on cancel
                if (Options.Cancel.State)
                    break;

                // Try to delete the file
                try
                {
                    Trace.WriteLine($"Deleting ({retryCount + 1} / {Options.FileRetryCount}) : \"{fileName}\"");
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    Trace.WriteLine(e);
                    Options.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    break;
                }
            }

            return result;
        }

        // Delete directory, and retry in case of failure
        public static bool DeleteDirectory(string directory)
        {
            // Test
            if (Options.TestNoModify)
                return true;

            bool result = false;
            for (int retryCount = 0; retryCount < Options.FileRetryCount; retryCount ++)
            {
                // Break on cancel
                if (Options.Cancel.State)
                    break;

                // Try to delete the directory
                try
                {
                    Trace.WriteLine($"Deleting ({retryCount + 1} / {Options.FileRetryCount}) : \"{directory}\"");
                    if (Directory.Exists(directory))
                        Directory.Delete(directory);
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // TODO : Do not retry if folder is not empty, it will never succeed

                    // Retry
                    Trace.WriteLine(e);
                    Options.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    break;
                }
            }

            return result;
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

        // Rename file, and retry in case of failure
        public static bool RenameFile(string originalName, string newName)
        {
            // Test
            if (Options.TestNoModify)
                return true;

            // Split path components so we can use them for pretty printing
            string originalDirectory = Path.GetDirectoryName(originalName);
            string originalFile = Path.GetFileName(originalName);
            string newDirectory = Path.GetDirectoryName(newName);
            string newFile = Path.GetFileName(newName);

            bool result = false;
            for (int retrycount = 0; retrycount < Options.FileRetryCount; retrycount ++)
            {
                // Break on cancel
                if (Options.Cancel.State)
                    break;

                // Try to rename the file
                // Delete the destination if it exists
                try
                {
                    Trace.WriteLine(originalDirectory.Equals(newDirectory, StringComparison.OrdinalIgnoreCase)
                        ? $"Renaming ({retrycount + 1} / {Options.FileRetryCount}) : \"{originalDirectory}\" : \"{originalFile}\" to \"{newFile}\""
                        : $"Renaming ({retrycount + 1} / {Options.FileRetryCount}) : \"{originalName}\" to \"{newName}\"");
                    if (File.Exists(newName))
                        File.Delete(newName);
                    File.Move(originalName, newName);
                    result = true;
                    break;
                }
                catch (FileNotFoundException e)
                {
                    // File not found
                    Trace.WriteLine(e);
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    Trace.WriteLine(e);
                    Options.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    break;
                }
            }

            return result;
        }

        // Rename or move the folder, and retry in case of failure
        public static bool RenameFolder(string originalName, string newName)
        {
            // Test
            if (Options.TestNoModify)
                return true;

            bool result = false;
            for (int retryCount = 0; retryCount < Options.FileRetryCount; retryCount ++)
            {
                // Break on cancel
                if (Options.Cancel.State)
                    break;

                // Try to move the folder
                try
                {
                    Trace.WriteLine($"Renaming ({retryCount + 1} / {Options.FileRetryCount}) : \"{originalName}\" to \"{newName}\"");
                    if (Directory.Exists(newName))
                        DeleteDirectory(newName, true);
                    Directory.Move(originalName, newName);
                    result = true;
                    break;
                }
                catch (FileNotFoundException e)
                {
                    // File not found
                    Trace.WriteLine(e);
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    Trace.WriteLine(e);
                    Options.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    break;
                }
            }

            return result;
        }

        // Recursively delete empty directories in the directory
        public static bool DeleteEmptyDirectories(string directory, ref int deleted)
        {
            // Find all directories in this directory, not all subdirectories, we will call recursively
            DirectoryInfo parentInfo = new DirectoryInfo(directory);
            foreach (DirectoryInfo dirInfo in parentInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                // Call recursively for this directory
                if (!DeleteEmptyDirectories(dirInfo.FullName, ref deleted))
                    return false;

                // Test for files and directories, if none, delete this directory
                if (dirInfo.GetFiles().Length != 0 || dirInfo.GetDirectories().Length != 0)
                    continue;

                if (!DeleteDirectory(dirInfo.FullName))
                    return false;
                deleted++;
            }

            return true;
        }

        // Recursively delete all the files and directories inside the directory
        public static bool DeleteInsideDirectory(string directory)
        {
            // Skip if directory does not exist
            if (!Directory.Exists(directory))
                return true;

            // Delete all files in this directory
            DirectoryInfo parentInfo = new DirectoryInfo(directory);
            if (parentInfo.GetFiles().Any(fileInfo => !DeleteFile(fileInfo.FullName)))
            {
                return false;
            }

            // Find all directories in this directory, not all subdirectories, we will call recursively
            foreach (DirectoryInfo dirInfo in parentInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                // Call recursively for this directory
                if (!DeleteInsideDirectory(dirInfo.FullName))
                    return false;

                // Delete this directory
                if (!DeleteDirectory(dirInfo.FullName))
                    return false;
            }

            return true;
        }

        // Try to open the file for read access
        public static bool IsFileReadable(string fileName)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                return IsFileReadable(fileInfo);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Wait for the file to become readable
        public static bool WaitFileReadAble(string fileName)
        {
            bool result = false;
            for (int retryCount = 0; retryCount < Options.FileRetryCount; retryCount ++)
            {
                // Break on cancel
                if (Options.Cancel.State)
                    break;

                // Try to access the file
                try
                {
                    Trace.WriteLine($"Waiting for file to become readable ({retryCount + 1} / {Options.FileRetryCount}) : \"{fileName}\"");
                    FileInfo fileinfo = new FileInfo(fileName);
                    FileStream stream = fileinfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                    stream.Close();
                    result = true;
                    break;
                }
                catch (IOException e)
                {
                    // Retry
                    Trace.WriteLine(e);
                    Options.WaitForCancelFileRetry();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    break;
                }
            }

            return result;
        }

        // Try to open the file for read access
        public static bool IsFileReadable(FileInfo fileInfo)
        {
            try
            {
                FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // Test if all files in the directory are readable
        public static bool AreFilesInDirectoryReadable(string directory)
        {
            // Test each file in directory for readability
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            return dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).All(IsFileReadable);
        }

        // Create directory if it does not already exists
        public static bool CreateDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
            return true;
        }

        // Combine paths and remove any extra root separators
        // Default Path.Combine() logic will treat rooted paths as only path
        public static string CombinePath(string path1, string path2)
        {
            // Trim roots from second path
            if (Path.IsPathRooted(path2))
            {
                path2 = path2.TrimStart(Path.DirectorySeparatorChar);
                path2 = path2.TrimStart(Path.AltDirectorySeparatorChar);
            }
            
            // Combine using normal logic, after stripping roots
            return Path.Combine(path1, path2);
        }

        // Enumerate all files and directories in the list of source directories
        // The source directories will be added to the directory list
        public static bool EnumerateDirectories(List<string> sourceList, out List<FileInfo> fileList, out List<DirectoryInfo> directoryList)
        {
            directoryList = new List<DirectoryInfo>();
            fileList = new List<FileInfo>();

            try
            {
                // Add all directories and files from the list of folders
                foreach (string folder in sourceList)
                {
                    // Add this folder to the directory list
                    DirectoryInfo dirInfo = new DirectoryInfo(folder);
                    directoryList.Add(dirInfo);

                    // Recursively add all child folders
                    directoryList.AddRange(dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories));
                }

                // Add all files from all the directories to the file list
                foreach (DirectoryInfo dirInfo in directoryList)
                {
                    // Add all files in this folder
                    fileList.AddRange(dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }

            return true;
        }

        // Enumerate all files and directories in the directory
        // This directory  will be added to the directory list
        public static bool EnumerateDirectory(string directory, out List<FileInfo> fileList, out List<DirectoryInfo> directoryList)
        {
            List<string> sourceList = new List<string> {directory};
            return EnumerateDirectories(sourceList, out fileList, out directoryList);
        }

        // Reset permissions on the directory
        // Grant everyone full control
        public static bool ResetDirectoryPermissions(string directory)
        {
            // Test
            if (Options.TestNoModify)
                return true;

            try
            {
                // Grant everyone full control
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                FileSystemAccessRule accessRule = new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow);
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                directorySecurity.AddAccessRule(accessRule);
                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
