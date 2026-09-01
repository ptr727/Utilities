namespace ptr727.Utilities.Tests;

public class DownloadTests
{
    [Fact]
    public void GetUriInformation()
    {
        using LoopbackServer server = new();

        _ = Download.GetContentInfo(server.OkUri, out long size, out DateTime _).Should().BeTrue();
        _ = size.Should().Be(LoopbackServer.ContentLength);
    }

    [Fact]
    public void DownloadFile_OverALongerExistingFile_ShouldReplaceIt()
    {
        using LoopbackServer server = new();
        string tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, new string('x', LoopbackServer.ContentLength * 4));

            _ = Download.DownloadFile(server.OkUri, tempFile).Should().BeTrue();

            // Opening rather than creating the file would leave the seeded bytes after the body.
            _ = File.ReadAllText(tempFile).Should().Be(LoopbackServer.Content);
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
    public void DownloadFile_WhenTheRequestFails_ShouldLeaveTheDestination()
    {
        using LoopbackServer server = new();
        string tempFile = Path.GetTempFileName();
        string existing = new('x', LoopbackServer.ContentLength * 4);

        try
        {
            File.WriteAllText(tempFile, existing);

            _ = Download.DownloadFile(server.MissingUri, tempFile).Should().BeFalse();

            // The destination is opened only once the response is accepted.
            // A request that never gets that far leaves it alone.
            _ = File.ReadAllText(tempFile).Should().Be(existing);
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
    public void DownloadFile_WithAnUnusableDestination_ShouldReturnFalse()
    {
        using LoopbackServer server = new();
        string missingDirectory = Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName(),
            "file.txt"
        );

        // A destination that cannot hold the file reports failure rather than throwing.
        _ = Download.DownloadFile(server.OkUri, missingDirectory).Should().BeFalse();

        // The body is fetched before the destination is opened.
        // The served request is what proves the failure came from the file rather than the request.
        _ = server.RequestCount.Should().Be(1);
    }

    [Fact]
    public void GetContentInfo_WithNotFoundUri_ShouldReturnFalse()
    {
        using LoopbackServer server = new();

        _ = Download
            .GetContentInfo(server.MissingUri, out long _, out DateTime _)
            .Should()
            .BeFalse();

        // A refused connection returns false too, so the failure is the route's only if it ran.
        _ = server.RequestCount.Should().Be(1);
    }
}
