using System.Collections.ObjectModel;

namespace ptr727.Utilities.Tests;

public sealed class FileExTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(),
        $"FileExTests-{Guid.NewGuid()}"
    );

    public FileExTests() => _ = Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private string NewFile(string name, string content = "content")
    {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private string NewDir(string name)
    {
        string path = Path.Combine(_tempDir, name);
        _ = Directory.CreateDirectory(path);
        return path;
    }

    [Fact]
    public void DeleteFile_ExistingFile_DeletesAndReturnsTrue()
    {
        string file = NewFile("delete.txt");

        _ = FileEx.DeleteFile(file).Should().BeTrue();
        _ = File.Exists(file).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_MissingFile_ReturnsTrue() =>
        _ = FileEx.DeleteFile(Path.Combine(_tempDir, "missing.txt")).Should().BeTrue();

    [Fact]
    public void DeleteDirectory_ExistingEmpty_DeletesAndReturnsTrue()
    {
        string dir = NewDir("empty");

        _ = FileEx.DeleteDirectory(dir).Should().BeTrue();
        _ = Directory.Exists(dir).Should().BeFalse();
    }

    [Fact]
    public void DeleteDirectory_Missing_ReturnsTrue() =>
        _ = FileEx.DeleteDirectory(Path.Combine(_tempDir, "missing")).Should().BeTrue();

    [Fact]
    public void DeleteDirectory_Recursive_DeletesContents()
    {
        string dir = NewDir("tree");
        string sub = Path.Combine(dir, "sub");
        _ = Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "file.txt"), "x");

        _ = FileEx.DeleteDirectory(dir, recursive: true).Should().BeTrue();
        _ = Directory.Exists(dir).Should().BeFalse();
    }

    [Fact]
    public void RenameFile_ToNewName_MovesFile()
    {
        string source = NewFile("source.txt", "data");
        string target = Path.Combine(_tempDir, "target.txt");

        _ = FileEx.RenameFile(source, target).Should().BeTrue();
        _ = File.Exists(source).Should().BeFalse();
        _ = File.ReadAllText(target).Should().Be("data");
    }

    [Fact]
    public void RenameFile_OverExistingDestination_Overwrites()
    {
        string source = NewFile("source.txt", "new");
        string target = NewFile("target.txt", "old");

        _ = FileEx.RenameFile(source, target).Should().BeTrue();
        _ = File.ReadAllText(target).Should().Be("new");
    }

    [Fact]
    public void RenameFile_SourceWithoutDirectory_ReturnsFalse() =>
        _ = FileEx
            .RenameFile("nodirectory.txt", Path.Combine(_tempDir, "x.txt"))
            .Should()
            .BeFalse();

    [Fact]
    public void RenameFile_MissingSource_ReturnsFalse() =>
        _ = FileEx
            .RenameFile(Path.Combine(_tempDir, "missing.txt"), Path.Combine(_tempDir, "x.txt"))
            .Should()
            .BeFalse();

    [Fact]
    public void RenameFolder_ToNewName_MovesFolder()
    {
        string source = NewDir("source");
        File.WriteAllText(Path.Combine(source, "file.txt"), "x");
        string target = Path.Combine(_tempDir, "target");

        _ = FileEx.RenameFolder(source, target).Should().BeTrue();
        _ = Directory.Exists(source).Should().BeFalse();
        _ = File.Exists(Path.Combine(target, "file.txt")).Should().BeTrue();
    }

    [Fact]
    public void RenameFolder_OverExistingDestination_Overwrites()
    {
        string source = NewDir("source");
        File.WriteAllText(Path.Combine(source, "new.txt"), "x");
        string target = NewDir("target");
        File.WriteAllText(Path.Combine(target, "old.txt"), "y");

        _ = FileEx.RenameFolder(source, target).Should().BeTrue();
        _ = File.Exists(Path.Combine(target, "new.txt")).Should().BeTrue();
        _ = File.Exists(Path.Combine(target, "old.txt")).Should().BeFalse();
    }

    [Fact]
    public void DeleteEmptyDirectories_RemovesEmptyAndCounts()
    {
        string dir = NewDir("root");
        _ = Directory.CreateDirectory(Path.Combine(dir, "emptyA"));
        _ = Directory.CreateDirectory(Path.Combine(dir, "emptyB"));

        int deleted = 0;
        _ = FileEx.DeleteEmptyDirectories(dir, ref deleted).Should().BeTrue();
        _ = deleted.Should().Be(2);
        _ = Directory.EnumerateDirectories(dir).Should().BeEmpty();
    }

    [Fact]
    public void DeleteEmptyDirectories_KeepsNonEmpty()
    {
        string dir = NewDir("root");
        string keep = Path.Combine(dir, "keep");
        _ = Directory.CreateDirectory(keep);
        File.WriteAllText(Path.Combine(keep, "file.txt"), "x");

        int deleted = 0;
        _ = FileEx.DeleteEmptyDirectories(dir, ref deleted).Should().BeTrue();
        _ = deleted.Should().Be(0);
        _ = Directory.Exists(keep).Should().BeTrue();
    }

    [Fact]
    public void DeleteInsideDirectory_RemovesContentsKeepsDirectory()
    {
        string dir = NewDir("root");
        File.WriteAllText(Path.Combine(dir, "file.txt"), "x");
        string sub = Path.Combine(dir, "sub");
        _ = Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "nested.txt"), "y");

        _ = FileEx.DeleteInsideDirectory(dir).Should().BeTrue();
        _ = Directory.Exists(dir).Should().BeTrue();
        _ = Directory.EnumerateFileSystemEntries(dir).Should().BeEmpty();
    }

    [Fact]
    public void DeleteInsideDirectory_Missing_ReturnsTrue() =>
        _ = FileEx.DeleteInsideDirectory(Path.Combine(_tempDir, "missing")).Should().BeTrue();

    [Fact]
    public void IsFileReadable_String_ReadableFile_ReturnsTrue() =>
        _ = FileEx.IsFileReadable(NewFile("readable.txt")).Should().BeTrue();

    [Fact]
    public void IsFileReadable_String_Missing_ReturnsFalse() =>
        _ = FileEx.IsFileReadable(Path.Combine(_tempDir, "missing.txt")).Should().BeFalse();

    [Fact]
    public void IsFileReadable_FileInfo_ReturnsTrue() =>
        _ = FileEx.IsFileReadable(new FileInfo(NewFile("read.txt"))).Should().BeTrue();

    [Fact]
    public void IsFileWriteable_FileInfo_ReturnsTrue() =>
        _ = FileEx.IsFileWriteable(new FileInfo(NewFile("write.txt"))).Should().BeTrue();

    [Fact]
    public void IsFileReadWriteable_FileInfo_ReturnsTrue() =>
        _ = FileEx.IsFileReadWriteable(new FileInfo(NewFile("readwrite.txt"))).Should().BeTrue();

    [Fact]
    public void IsFileReadable_NullFileInfo_Throws() =>
        _ = FluentActions
            .Invoking(() => FileEx.IsFileReadable((FileInfo)null!))
            .Should()
            .Throw<ArgumentNullException>();

    [Fact]
    public void WaitFileReadable_ReadableFile_ReturnsTrue() =>
        _ = FileEx.WaitFileReadable(NewFile("wait.txt")).Should().BeTrue();

    [Fact]
    public void AreFilesInDirectoryReadable_AllReadable_ReturnsTrue()
    {
        _ = NewFile("a.txt");
        _ = NewFile("b.txt");

        _ = FileEx.AreFilesInDirectoryReadable(_tempDir).Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_New_CreatesIt()
    {
        string dir = Path.Combine(_tempDir, "created");

        _ = FileEx.CreateDirectory(dir).Should().BeTrue();
        _ = Directory.Exists(dir).Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_Existing_ReturnsTrue() =>
        _ = FileEx.CreateDirectory(_tempDir).Should().BeTrue();

    [Fact]
    public void EnumerateDirectory_ReturnsFilesAndDirectories()
    {
        _ = NewFile("file.txt");
        string sub = NewDir("sub");

        bool result = FileEx.EnumerateDirectory(
            _tempDir,
            out Collection<FileInfo> files,
            out Collection<DirectoryInfo> directories
        );

        _ = result.Should().BeTrue();
        _ = directories.Select(d => d.FullName).Should().Contain([_tempDir, sub]);
        _ = files.Select(f => f.Name).Should().Contain("file.txt");
    }

    [Fact]
    public void EnumerateDirectories_MultipleSources_ReturnsAll()
    {
        string dirA = NewDir("A");
        string dirB = NewDir("B");
        File.WriteAllText(Path.Combine(dirA, "a.txt"), "x");
        File.WriteAllText(Path.Combine(dirB, "b.txt"), "y");

        bool result = FileEx.EnumerateDirectories(
            [dirA, dirB],
            out Collection<FileInfo> files,
            out Collection<DirectoryInfo> directories
        );

        _ = result.Should().BeTrue();
        _ = directories.Select(d => d.FullName).Should().Contain([dirA, dirB]);
        _ = files.Select(f => f.Name).Should().Contain(["a.txt", "b.txt"]);
    }

    [Fact]
    public void TimeStampFileName_PrefixesTimestamp()
    {
        string path = Path.Combine(_tempDir, "report.log");
        DateTime timeStamp = new(2026, 7, 10, 14, 30, 15, DateTimeKind.Utc);

        string result = FileEx.TimeStampFileName(path, timeStamp);

        _ = Path.GetFileName(result).Should().Be("20260710T143015_report.log");
        _ = Path.GetDirectoryName(result).Should().Be(_tempDir);
    }

    [Fact]
    public void TimeStampFileName_DefaultTimestamp_PrefixesFileName() =>
        _ = Path.GetFileName(FileEx.TimeStampFileName(Path.Combine(_tempDir, "report.log")))
            .Should()
            .EndWith("_report.log");

    [Fact]
    public void CreateRandomFilledFile_CreatesFileOfSize()
    {
        string path = Path.Combine(_tempDir, "random.bin");

        _ = FileEx.CreateRandomFilledFile(path, 2048).Should().BeTrue();
        _ = new FileInfo(path).Length.Should().Be(2048);
    }

    [Fact]
    public void CreateSparseFile_CreatesFileOfSize()
    {
        string path = Path.Combine(_tempDir, "sparse.bin");

        _ = FileEx.CreateSparseFile(path, 4096).Should().BeTrue();
        _ = new FileInfo(path).Length.Should().Be(4096);
    }
}
