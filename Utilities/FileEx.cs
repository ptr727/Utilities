using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace InsaneGenius.Utilities;

public static class FileEx
{
    // Settings for file operations
    public static readonly FileExOptions Options = new();

    // Delete file, and retry in case of failure
    public static bool DeleteFile(string fileName)
    {
        // Test
        if (Options.TestNoModify)
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            // Break on cancel
            if (Options.Cancel.IsCancellationRequested)
            {
                break;
            }

            // Try to delete the file
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                result = true;
                break;
            }
            catch (IOException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // Retry
                LogOptions.Logger.Information(
                    "Deleting ({RetryCount} / {OptionsRetryCount}) : {FileName}",
                    retryCount,
                    Options.RetryCount,
                    fileName
                );
                _ = Options.RetryWaitForCancel();
            }
            catch (Exception e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
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
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            // Break on cancel
            if (Options.Cancel.IsCancellationRequested)
            {
                break;
            }

            // Try to delete the directory
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory);
                }

                result = true;
                break;
            }
            catch (IOException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // TODO : Do not retry if folder is not empty, it will never succeed

                // Retry
                LogOptions.Logger.Information(
                    "Deleting ({RetryCount} / {OptionsRetryCount}) : {Directory}",
                    retryCount,
                    Options.RetryCount,
                    directory
                );
                _ = Options.RetryWaitForCancel();
            }
            catch (Exception e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
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
        {
            return DeleteDirectory(directory);
        }

        // Recursively delete inside the directory and the directory
        return DeleteInsideDirectory(directory) && DeleteDirectory(directory);
    }

    // Rename file, and retry in case of failure
    public static bool RenameFile(string originalName, string newName)
    {
        // Test
        if (Options.TestNoModify)
        {
            return true;
        }

        // Split path components so we can use them for pretty printing
        string originalDirectory = Path.GetDirectoryName(originalName);
        string originalFile = Path.GetFileName(originalName);
        string newDirectory = Path.GetDirectoryName(newName);
        string newFile = Path.GetFileName(newName);

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            // Break on cancel
            if (Options.Cancel.IsCancellationRequested)
            {
                break;
            }

            // Try to rename the file
            // Delete the destination if it exists
            try
            {
                if (File.Exists(newName))
                {
                    File.Delete(newName);
                }

                File.Move(originalName, newName);
                result = true;
                break;
            }
            catch (FileNotFoundException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // File not found
                break;
            }
            catch (IOException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // Retry
                if (originalDirectory.Equals(newDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    LogOptions.Logger.Information(
                        "Renaming ({RetryCount} / {OptionsRetryCount}) : {OriginalDirectory} : {OriginalFile} to {NewFile}",
                        retryCount,
                        Options.RetryCount,
                        originalDirectory,
                        originalFile,
                        newFile
                    );
                }
                else
                {
                    LogOptions.Logger.Information(
                        "Renaming ({RetryCount} / {OptionsRetryCount}) : {OriginalName} to {NewName}",
                        retryCount,
                        Options.RetryCount,
                        originalName,
                        newName
                    );
                }

                _ = Options.RetryWaitForCancel();
            }
            catch (Exception e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
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
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            // Break on cancel
            if (Options.Cancel.IsCancellationRequested)
            {
                break;
            }

            // Try to move the folder
            try
            {
                if (Directory.Exists(newName))
                {
                    _ = DeleteDirectory(newName, true);
                }

                Directory.Move(originalName, newName);
                result = true;
                break;
            }
            catch (FileNotFoundException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // File not found
                break;
            }
            catch (IOException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // Retry
                LogOptions.Logger.Information(
                    "Renaming ({RetryCount} / {OptionsRetryCount}) : {OriginalName} to {NewName}",
                    retryCount,
                    Options.RetryCount,
                    originalName,
                    newName
                );
                _ = Options.RetryWaitForCancel();
            }
            catch (Exception e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                break;
            }
        }

        return result;
    }

    // Recursively delete empty directories in the directory
    public static bool DeleteEmptyDirectories(string directory, ref int deleted)
    {
        // Find all directories in this directory, not all subdirectories, we will call recursively
        DirectoryInfo parentInfo = new(directory);
        foreach (
            DirectoryInfo dirInfo in parentInfo.EnumerateDirectories(
                "*",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            // Call recursively for this directory
            if (!DeleteEmptyDirectories(dirInfo.FullName, ref deleted))
            {
                return false;
            }

            // Test for files and directories, if none, delete this directory
            if (dirInfo.GetFiles().Length != 0 || dirInfo.GetDirectories().Length != 0)
            {
                continue;
            }

            if (!DeleteDirectory(dirInfo.FullName))
            {
                return false;
            }

            deleted++;
        }

        return true;
    }

    // Recursively delete all the files and directories inside the directory
    public static bool DeleteInsideDirectory(string directory)
    {
        // Skip if directory does not exist
        if (!Directory.Exists(directory))
        {
            return true;
        }

        // Delete all files in this directory
        DirectoryInfo parentInfo = new(directory);
        if (parentInfo.GetFiles().Any(fileInfo => !DeleteFile(fileInfo.FullName)))
        {
            return false;
        }

        // Find all directories in this directory, not all subdirectories, we will call recursively
        foreach (
            DirectoryInfo dirInfo in parentInfo.EnumerateDirectories(
                "*",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            // Call recursively for this directory
            if (!DeleteInsideDirectory(dirInfo.FullName))
            {
                return false;
            }

            // Delete this directory
            if (!DeleteDirectory(dirInfo.FullName))
            {
                return false;
            }
        }

        return true;
    }

    // Try to open the file for read access
    public static bool IsFileReadable(string fileName)
    {
        try
        {
            FileInfo fileInfo = new(fileName);
            return IsFileReadable(fileInfo);
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }
    }

    // Wait for the file to become readable
    public static bool WaitFileReadAble(string fileName)
    {
        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            // Break on cancel
            if (Options.Cancel.IsCancellationRequested)
            {
                break;
            }

            // Try to access the file
            try
            {
                FileInfo fileInfo = new(fileName);
                using FileStream stream = fileInfo.Open(
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                stream.Close();
                result = true;
                break;
            }
            catch (IOException e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                // Retry
                LogOptions.Logger.Information(
                    "Waiting for file to become readable ({RetryCount} / {OptionsRetryCount}) : {Name}",
                    retryCount,
                    Options.RetryCount,
                    fileName
                );
                _ = Options.RetryWaitForCancel();
            }
            catch (Exception e)
                when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
            {
                break;
            }
        }

        return result;
    }

    // Try to open the file for read access
    public static bool IsFileReadable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            // Try to open the file for read access
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            stream.Close();
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Try to open the file for write access
    public static bool IsFileWriteable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            // Try to open the file for write access
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite
            );
            stream.Close();
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Try to open the file for read and write access
    public static bool IsFileReadWriteable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            // Try to open the file for read-write access
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );
            stream.Close();
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Test if all files in the directory are readable
    public static bool AreFilesInDirectoryReadable(string directory)
    {
        try
        {
            // Test each file in directory for readability
            DirectoryInfo dirInfo = new(directory);
            return dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).All(IsFileReadable);
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }
    }

    // Create directory if it does not already exists
    public static bool CreateDirectory(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Combine paths and convert relative to absolute
    public static string CombinePath(string path1, string path2, string path3)
    {
        ArgumentNullException.ThrowIfNull(path1);
        ArgumentNullException.ThrowIfNull(path2);
        ArgumentNullException.ThrowIfNull(path3);

        // Trim roots from second and third path
        if (Path.IsPathRooted(path2))
        {
            path2 = path2.TrimStart(Path.DirectorySeparatorChar);
            path2 = path2.TrimStart(Path.AltDirectorySeparatorChar);
        }
        if (Path.IsPathRooted(path3))
        {
            path3 = path3.TrimStart(Path.DirectorySeparatorChar);
            path3 = path3.TrimStart(Path.AltDirectorySeparatorChar);
        }

        // Combine and convert relative paths to absolute local paths
        // Uri.LocalPath will add slashes at the end of directories
        // Uri.LocalPath will lowercase UNC server names
        string path = new Uri(Path.Combine(path1, path2, path3)).LocalPath;

        // Remove directory separator from path end
        path = path.TrimEnd(Path.DirectorySeparatorChar);
        path = path.TrimEnd(Path.AltDirectorySeparatorChar);

        return path;
    }

    public static string CombinePath(string path1, string path2)
    {
        ArgumentNullException.ThrowIfNull(path1);
        ArgumentNullException.ThrowIfNull(path2);

        return CombinePath(path1, path2, "");
    }

    // Enumerate all files and directories in the list of source directories
    // The source directories will be added to the directory list
    public static bool EnumerateDirectories(
        List<string> sourceList,
        out List<FileInfo> fileList,
        out List<DirectoryInfo> directoryList
    )
    {
        ArgumentNullException.ThrowIfNull(sourceList);

        directoryList = [];
        fileList = [];

        try
        {
            // Add all directories and files from the list of folders
            foreach (string folder in sourceList)
            {
                // Add this folder to the directory list
                DirectoryInfo dirInfo = new(folder);
                directoryList.Add(dirInfo);

                // Recursively add all child folders
                directoryList.AddRange(
                    dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                );
            }

            // Add all files from all the directories to the file list
            foreach (DirectoryInfo dirInfo in directoryList)
            {
                // Add all files in this folder
                fileList.AddRange(dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));
            }
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Enumerate all files and directories in the directory
    // This directory  will be added to the directory list
    public static bool EnumerateDirectory(
        string directory,
        out List<FileInfo> fileList,
        out List<DirectoryInfo> directoryList
    )
    {
        List<string> sourceList = [directory];
        return EnumerateDirectories(sourceList, out fileList, out directoryList);
    }

    // Reset permissions on the directory
    // Grant everyone full control
    public static bool ResetDirectoryPermissions(string directory)
    {
        // Test
        if (Options.TestNoModify)
        {
            return true;
        }

        // Windows only
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return true;
        }

        try
        {
            // Grant everyone full control
            DirectoryInfo directoryInfo = new(directory);
            SecurityIdentifier everyone = new(WellKnownSidType.WorldSid, null);
            FileSystemAccessRule accessRule = new(
                everyone,
                FileSystemRights.FullControl,
                AccessControlType.Allow
            );
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity.AddAccessRule(accessRule);
            directoryInfo.SetAccessControl(directorySecurity);
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    // Prefix the filename with a timestamp
    public static string TimeStampFileName(string filePath) =>
        TimeStampFileName(filePath, DateTime.UtcNow);

    public static string TimeStampFileName(string filePath, DateTime timeStamp)
    {
        string directory = Path.GetDirectoryName(filePath);
        string fileName = $"{timeStamp:yyyyMMddTHHmmss}_{Path.GetFileName(filePath)}";
        return Path.Combine(directory, fileName);
    }

    public static bool CreateRandomFilledFile(string name, long size)
    {
        try
        {
            // Create the file
            using FileStream stream = File.Open(
                name,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            // Set the file length, this has no real impact on COW filesystems
            stream.SetLength(size);
            _ = stream.Seek(0, SeekOrigin.Begin);

            // Buffer with random data
            const int bufferSize = 2 * Format.MiB;
            byte[] buffer = new byte[bufferSize];
            Random rand = new();

            // Write in buffer chunks
            long remaining = size;
            while (remaining > 0)
            {
                // Fill buffer with random data
                rand.NextBytes(buffer);
                // Write
                long writeSize = Math.Min(remaining, Convert.ToInt64(buffer.Length));
                stream.Write(buffer, 0, Convert.ToInt32(writeSize));
                // Remaining
                remaining -= writeSize;
            }

            // Close
            stream.Close();
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }

    public static bool CreateSparseFile(string name, long size)
    {
        try
        {
            // Create the file
            using FileStream stream = File.Open(
                name,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            // Set length
            stream.SetLength(size);

            // Close
            stream.Close();
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        return true;
    }
}
