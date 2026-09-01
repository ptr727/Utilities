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
    public void GetContentInfo_WithNotFoundUri_ShouldReturnFalse()
    {
        using LoopbackServer server = new();

        _ = Download
            .GetContentInfo(server.MissingUri, out long _, out DateTime _)
            .Should()
            .BeFalse();
    }
}
