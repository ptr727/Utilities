using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides extended file and directory operation utilities with retry logic and cancellation support.
/// </summary>
public static class FileEx
{
    /// <summary>
    /// Settings for file operations including retry behavior and cancellation.
    /// </summary>
    public static readonly FileExOptions Options = new();

    /// <summary>
    /// Deletes a file with retry logic in case of failure.
    /// </summary>
    /// <param name="fileName">The file to delete.</param>
    /// <returns>True if successful, false otherwise.</returns>
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
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes a file asynchronously with retry logic in case of failure.
    /// </summary>
    /// <param name="fileName">The file to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> DeleteFileAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        if (Options.TestNoModify)
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            if (Options.Cancel.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                result = true;
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                LogOptions.Logger.Information(
                    "Deleting ({RetryCount} / {OptionsRetryCount}) : {FileName}",
                    retryCount,
                    Options.RetryCount,
                    fileName
                );
                await Task.Delay(Options.RetryWaitTime * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes a directory with retry logic in case of failure.
    /// </summary>
    /// <param name="directory">The directory to delete.</param>
    /// <returns>True if successful, false otherwise.</returns>
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
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes a directory asynchronously with retry logic in case of failure.
    /// </summary>
    /// <param name="directory">The directory to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> DeleteDirectoryAsync(
        string directory,
        CancellationToken cancellationToken = default
    )
    {
        if (Options.TestNoModify)
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            if (Options.Cancel.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory);
                }

                result = true;
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                LogOptions.Logger.Information(
                    "Deleting ({RetryCount} / {OptionsRetryCount}) : {Directory}",
                    retryCount,
                    Options.RetryCount,
                    directory
                );
                await Task.Delay(Options.RetryWaitTime * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Recursively deletes a directory and all its contents.
    /// </summary>
    /// <param name="directory">The directory to delete.</param>
    /// <param name="recursive">If true, recursively deletes all contents before deleting the directory.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Recursively deletes a directory and all its contents asynchronously.
    /// </summary>
    /// <param name="directory">The directory to delete.</param>
    /// <param name="recursive">If true, recursively deletes all contents before deleting the directory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> DeleteDirectoryAsync(
        string directory,
        bool recursive,
        CancellationToken cancellationToken = default
    ) =>
        !recursive
            ? await DeleteDirectoryAsync(directory, cancellationToken).ConfigureAwait(false)
            : await DeleteInsideDirectoryAsync(directory, cancellationToken).ConfigureAwait(false)
                && await DeleteDirectoryAsync(directory, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Renames or moves a file with retry logic in case of failure.
    /// </summary>
    /// <param name="originalName">The original file path.</param>
    /// <param name="newName">The new file path.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool RenameFile(string originalName, string newName)
    {
        // Test
        if (Options.TestNoModify)
        {
            return true;
        }

        // Split path components so we can use them for pretty printing
        string? originalDirectory = Path.GetDirectoryName(originalName);
        string originalFile = Path.GetFileName(originalName);
        string? newDirectory = Path.GetDirectoryName(newName);
        string newFile = Path.GetFileName(newName);
        if (
            string.IsNullOrEmpty(originalDirectory)
            || string.IsNullOrEmpty(originalFile)
            || string.IsNullOrEmpty(newDirectory)
            || string.IsNullOrEmpty(newFile)
        )
        {
            LogOptions.Logger.Error(
                "Renaming file failed due to invalid path(s) : {OriginalName} to {NewName}",
                originalName,
                newName
            );
            return false;
        }

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
            catch (FileNotFoundException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // File not found
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Renames or moves a file asynchronously with retry logic in case of failure.
    /// </summary>
    /// <param name="originalName">The original file path.</param>
    /// <param name="newName">The new file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> RenameFileAsync(
        string originalName,
        string newName,
        CancellationToken cancellationToken = default
    )
    {
        if (Options.TestNoModify)
        {
            return true;
        }

        string? originalDirectory = Path.GetDirectoryName(originalName);
        string originalFile = Path.GetFileName(originalName);
        string? newDirectory = Path.GetDirectoryName(newName);
        string newFile = Path.GetFileName(newName);
        if (
            string.IsNullOrEmpty(originalDirectory)
            || string.IsNullOrEmpty(originalFile)
            || string.IsNullOrEmpty(newDirectory)
            || string.IsNullOrEmpty(newFile)
        )
        {
            LogOptions.Logger.Error(
                "Renaming file failed due to invalid path(s) : {OriginalName} to {NewName}",
                originalName,
                newName
            );
            return false;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            if (Options.Cancel.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                break;
            }

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
            catch (FileNotFoundException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // File not found
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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

                await Task.Delay(Options.RetryWaitTime * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Renames or moves a folder with retry logic in case of failure.
    /// </summary>
    /// <param name="originalName">The original folder path.</param>
    /// <param name="newName">The new folder path.</param>
    /// <returns>True if successful, false otherwise.</returns>
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
            catch (FileNotFoundException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // File not found
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Renames or moves a folder asynchronously with retry logic in case of failure.
    /// </summary>
    /// <param name="originalName">The original folder path.</param>
    /// <param name="newName">The new folder path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> RenameFolderAsync(
        string originalName,
        string newName,
        CancellationToken cancellationToken = default
    )
    {
        if (Options.TestNoModify)
        {
            return true;
        }

        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            if (Options.Cancel.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (Directory.Exists(newName))
                {
                    _ = await DeleteDirectoryAsync(newName, true, cancellationToken)
                        .ConfigureAwait(false);
                }

                Directory.Move(originalName, newName);
                result = true;
                break;
            }
            catch (FileNotFoundException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // File not found
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // Retry
                LogOptions.Logger.Information(
                    "Renaming ({RetryCount} / {OptionsRetryCount}) : {OriginalName} to {NewName}",
                    retryCount,
                    Options.RetryCount,
                    originalName,
                    newName
                );
                await Task.Delay(Options.RetryWaitTime * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Recursively deletes empty directories within a directory.
    /// </summary>
    /// <param name="directory">The directory to process.</param>
    /// <param name="deleted">Reference to a counter that will be incremented for each deleted directory.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Recursively deletes all files and directories inside a directory, but not the directory itself.
    /// </summary>
    /// <param name="directory">The directory to clean.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Recursively deletes all files and directories inside a directory asynchronously, but not the directory itself.
    /// </summary>
    /// <param name="directory">The directory to clean.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> DeleteInsideDirectoryAsync(
        string directory,
        CancellationToken cancellationToken = default
    )
    {
        if (!Directory.Exists(directory))
        {
            return true;
        }

        DirectoryInfo parentInfo = new(directory);

        // Delete all files in this directory
        foreach (FileInfo fileInfo in parentInfo.GetFiles())
        {
            if (!await DeleteFileAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }
        }

        // Find all directories in this directory, not all subdirectories, we will call recursively
        foreach (
            DirectoryInfo dirInfo in parentInfo.EnumerateDirectories(
                "*",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            if (
                !await DeleteInsideDirectoryAsync(dirInfo.FullName, cancellationToken)
                    .ConfigureAwait(false)
            )
            {
                return false;
            }

            if (
                !await DeleteDirectoryAsync(dirInfo.FullName, cancellationToken)
                    .ConfigureAwait(false)
            )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a file is readable.
    /// </summary>
    /// <param name="fileName">The file path to check.</param>
    /// <returns>True if the file can be opened for reading, false otherwise.</returns>
    public static bool IsFileReadable(string fileName)
    {
        try
        {
            FileInfo fileInfo = new(fileName);
            return IsFileReadable(fileInfo);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for a file to become readable with retry logic.
    /// </summary>
    /// <param name="fileName">The file path to wait for.</param>
    /// <returns>True if the file became readable, false otherwise.</returns>
    public static bool WaitFileReadable(string fileName)
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
                result = true;
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
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
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Waits for a file to become readable asynchronously with retry logic.
    /// </summary>
    /// <param name="fileName">The file path to wait for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the file became readable, false otherwise.</returns>
    public static async Task<bool> WaitFileReadableAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        bool result = false;
        for (int retryCount = 0; retryCount < Options.RetryCount; retryCount++)
        {
            if (Options.Cancel.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Try to access the file
            try
            {
                FileInfo fileInfo = new(fileName);
                await using FileStream stream = fileInfo.Open(
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                result = true;
                break;
            }
            catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
            {
                // Retry
                LogOptions.Logger.Information(
                    "Waiting for file to become readable ({RetryCount} / {OptionsRetryCount}) : {Name}",
                    retryCount,
                    Options.RetryCount,
                    fileName
                );
                await Task.Delay(Options.RetryWaitTime * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a file is readable.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object to check.</param>
    /// <returns>True if the file can be opened for reading, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileInfo"/> is null.</exception>
    public static bool IsFileReadable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a file is writeable.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object to check.</param>
    /// <returns>True if the file can be opened for writing, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileInfo"/> is null.</exception>
    public static bool IsFileWriteable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite
            );
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a file is both readable and writeable.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object to check.</param>
    /// <returns>True if the file can be opened for reading and writing, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileInfo"/> is null.</exception>
    public static bool IsFileReadWriteable(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        try
        {
            using FileStream stream = fileInfo.Open(
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Tests if all files in a directory are readable.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>True if all files are readable, false otherwise.</returns>
    public static bool AreFilesInDirectoryReadable(string directory)
    {
        try
        {
            // Test each file in directory for readability
            DirectoryInfo dirInfo = new(directory);
            return dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).All(IsFileReadable);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a directory if it does not already exist.
    /// </summary>
    /// <param name="directory">The directory path to create.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool CreateDirectory(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Combines three paths and converts the result to an absolute path.
    /// </summary>
    /// <param name="path1">The first path component.</param>
    /// <param name="path2">The second path component.</param>
    /// <param name="path3">The third path component.</param>
    /// <returns>The combined absolute path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
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

    /// <summary>
    /// Combines two paths and converts the result to an absolute path.
    /// </summary>
    /// <param name="path1">The first path component.</param>
    /// <param name="path2">The second path component.</param>
    /// <returns>The combined absolute path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static string CombinePath(string path1, string path2)
    {
        ArgumentNullException.ThrowIfNull(path1);
        ArgumentNullException.ThrowIfNull(path2);

        return CombinePath(path1, path2, "");
    }

    /// <summary>
    /// Enumerates all files and directories in the specified source directories.
    /// </summary>
    /// <param name="sourceList">The list of source directories to enumerate.</param>
    /// <param name="fileList">Output list of all files found.</param>
    /// <param name="directoryList">Output list of all directories found (includes source directories).</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceList"/> is null.</exception>
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Enumerates all files and directories in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to enumerate.</param>
    /// <param name="fileList">Output list of all files found.</param>
    /// <param name="directoryList">Output list of all directories found (includes the specified directory).</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool EnumerateDirectory(
        string directory,
        out List<FileInfo> fileList,
        out List<DirectoryInfo> directoryList
    )
    {
        List<string> sourceList = [directory];
        return EnumerateDirectories(sourceList, out fileList, out directoryList);
    }

    /// <summary>
    /// Resets directory permissions to grant everyone full control (Windows only).
    /// </summary>
    /// <param name="directory">The directory to reset permissions on.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <remarks>
    /// ⚠️ WARNING: This grants Everyone full control to the directory.
    /// Only use this for temporary directories or when you understand the security implications.
    /// This method only executes on Windows platforms.
    /// </remarks>
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prefixes a file name with the current UTC timestamp.
    /// </summary>
    /// <param name="filePath">The original file path.</param>
    /// <returns>The file path with timestamp prefix.</returns>
    public static string TimeStampFileName(string filePath) =>
        TimeStampFileName(filePath, DateTime.UtcNow);

    /// <summary>
    /// Prefixes a file name with a specified timestamp.
    /// </summary>
    /// <param name="filePath">The original file path.</param>
    /// <param name="timeStamp">The timestamp to use for prefixing.</param>
    /// <returns>The file path with timestamp prefix.</returns>
    public static string TimeStampFileName(string filePath, DateTime timeStamp)
    {
        string? directory = Path.GetDirectoryName(filePath);
        string fileName = $"{timeStamp:yyyyMMddTHHmmss}_{Path.GetFileName(filePath)}";
        return Path.Combine(directory!, fileName);
    }

    /// <summary>
    /// Creates a file filled with random data.
    /// </summary>
    /// <param name="name">The file path to create.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <returns>True if successful, false otherwise.</returns>
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
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a file filled with random data asynchronously.
    /// </summary>
    /// <param name="name">The file path to create.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> CreateRandomFilledFileAsync(
        string name,
        long size,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await using FileStream stream = File.Open(
                name,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            stream.SetLength(size);
            _ = stream.Seek(0, SeekOrigin.Begin);

            const int bufferSize = 2 * Format.MiB;
            byte[] buffer = new byte[bufferSize];
            Random rand = new();

            long remaining = size;
            while (remaining > 0)
            {
                rand.NextBytes(buffer);
                long writeSize = Math.Min(remaining, Convert.ToInt64(buffer.Length));
                await stream
                    .WriteAsync(buffer.AsMemory(0, Convert.ToInt32(writeSize)), cancellationToken)
                    .ConfigureAwait(false);
                remaining -= writeSize;
            }
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a sparse file of the specified size.
    /// </summary>
    /// <param name="name">The file path to create.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <returns>True if successful, false otherwise.</returns>
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
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a sparse file of the specified size asynchronously.
    /// </summary>
    /// <param name="name">The file path to create.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static async Task<bool> CreateSparseFileAsync(string name, long size)
    {
        try
        {
            await using FileStream stream = File.Open(
                name,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            stream.SetLength(size);
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
        {
            return false;
        }

        return true;
    }
}
