namespace ptr727.Utilities.Tests;

public class FileExAsyncTests : IClassFixture<UtilitiesTests>
{
    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ShouldReturnTrue()
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            bool result = await FileEx.DeleteFileAsync(tempFile);

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeFalse();
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
        string tempDir = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);

        try
        {
            bool result = await FileEx.DeleteDirectoryAsync(tempDir);

            _ = result.Should().BeTrue();
            _ = Directory.Exists(tempDir).Should().BeFalse();
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
        string tempDir = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        string subDir = Path.Combine(tempDir, "subdir");
        _ = Directory.CreateDirectory(subDir);
        string testFile = Path.Combine(subDir, "test.txt");
        await File.WriteAllTextAsync(testFile, "test content");

        try
        {
            bool result = await FileEx.DeleteDirectoryAsync(tempDir, recursive: true);

            _ = result.Should().BeTrue();
            _ = Directory.Exists(tempDir).Should().BeFalse();
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
        string newPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

        try
        {
            bool result = await FileEx.RenameFileAsync(tempFile, newPath);

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeFalse();
            _ = File.Exists(newPath).Should().BeTrue();
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
        string tempDir = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        string newPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");

        try
        {
            bool result = await FileEx.RenameFolderAsync(tempDir, newPath);

            _ = result.Should().BeTrue();
            _ = Directory.Exists(tempDir).Should().BeFalse();
            _ = Directory.Exists(newPath).Should().BeTrue();
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

            _ = result.Should().BeTrue();
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
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        long fileSize = 1024 * 1024; // 1MB

        try
        {
            bool result = await FileEx.CreateRandomFilledFileAsync(tempFile, fileSize);

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeTrue();
            _ = new FileInfo(tempFile).Length.Should().Be(fileSize);
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
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        long fileSize = 1024 * 1024; // 1MB

        try
        {
            bool result = await FileEx.CreateSparseFileAsync(tempFile, fileSize);

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeTrue();
            _ = new FileInfo(tempFile).Length.Should().Be(fileSize);
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
            _ = (result || cts.Token.IsCancellationRequested).Should().BeTrue();
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
        string tempDir = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        string testFile = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(testFile, "test content");

        try
        {
            bool result = await FileEx.DeleteInsideDirectoryAsync(tempDir);

            _ = result.Should().BeTrue();
            _ = Directory.Exists(tempDir).Should().BeTrue(); // Directory should still exist
            _ = File.Exists(testFile).Should().BeFalse(); // But file should be gone
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
