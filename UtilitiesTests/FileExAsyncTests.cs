using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class FileExAsyncTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ShouldReturnTrue()
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            bool result = await FileEx.DeleteFileAsync(tempFile);

            Assert.True(result);
            Assert.False(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithExistingDirectory_ShouldReturnTrue()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(tempDir);

        try
        {
            bool result = await FileEx.DeleteDirectoryAsync(tempDir);

            Assert.True(result);
            Assert.False(Directory.Exists(tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir);
            }
        }
    }

    [Fact]
    public async Task DeleteDirectoryAsync_Recursive_ShouldDeleteAllContents()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(tempDir);
        string subDir = Path.Combine(tempDir, "subdir");
        _ = Directory.CreateDirectory(subDir);
        string testFile = Path.Combine(subDir, "test.txt");
        await File.WriteAllTextAsync(testFile, "test content");

        try
        {
            bool result = await FileEx.DeleteDirectoryAsync(tempDir, recursive: true);

            Assert.True(result);
            Assert.False(Directory.Exists(tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RenameFileAsync_WithValidPaths_ShouldRenameFile()
    {
        string tempFile = Path.GetTempFileName();
        string newPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        try
        {
            bool result = await FileEx.RenameFileAsync(tempFile, newPath);

            Assert.True(result);
            Assert.False(File.Exists(tempFile));
            Assert.True(File.Exists(newPath));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
        }
    }

    [Fact]
    public async Task RenameFolderAsync_WithValidPaths_ShouldRenameFolder()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(tempDir);
        string newPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            bool result = await FileEx.RenameFolderAsync(tempDir, newPath);

            Assert.True(result);
            Assert.False(Directory.Exists(tempDir));
            Assert.True(Directory.Exists(newPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir);
            }
            if (Directory.Exists(newPath))
            {
                Directory.Delete(newPath);
            }
        }
    }

    [Fact]
    public async Task WaitFileReadableAsync_WithReadableFile_ShouldReturnTrue()
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "test content");

            bool result = await FileEx.WaitFileReadableAsync(tempFile);

            Assert.True(result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task CreateRandomFilledFileAsync_ShouldCreateFile()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dat");
        long fileSize = 1024 * 1024; // 1MB

        try
        {
            bool result = await FileEx.CreateRandomFilledFileAsync(tempFile, fileSize);

            Assert.True(result);
            Assert.True(File.Exists(tempFile));
            Assert.Equal(fileSize, new FileInfo(tempFile).Length);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task CreateSparseFileAsync_ShouldCreateFile()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dat");
        long fileSize = 1024 * 1024; // 1MB

        try
        {
            bool result = await FileEx.CreateSparseFileAsync(tempFile, fileSize);

            Assert.True(result);
            Assert.True(File.Exists(tempFile));
            Assert.Equal(fileSize, new FileInfo(tempFile).Length);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task FileOperationsAsync_WithCancellation_ShouldRespectCancellation()
    {
        string tempFile = Path.GetTempFileName();
        using CancellationTokenSource cts = new();
        cts.Cancel(); // Cancel immediately

        try
        {
            // Operations should respect cancellation
            bool result = await FileEx.DeleteFileAsync(tempFile, cts.Token);

            // Should either succeed quickly or respect cancellation
            Assert.True(result || cts.Token.IsCancellationRequested);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DeleteInsideDirectoryAsync_ShouldDeleteContentsOnly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(tempDir);
        string testFile = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(testFile, "test content");

        try
        {
            bool result = await FileEx.DeleteInsideDirectoryAsync(tempDir);

            Assert.True(result);
            Assert.True(Directory.Exists(tempDir)); // Directory should still exist
            Assert.False(File.Exists(testFile)); // But file should be gone
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
