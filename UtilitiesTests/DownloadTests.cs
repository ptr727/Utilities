namespace ptr727.Utilities.Tests;

public class DownloadTests
{
    [Fact]
    public void GetUriInformation()
    {
        Uri uri = new(
            "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
        );
        _ = Download.GetContentInfo(uri, out long _, out DateTime _).Should().BeTrue();
    }
}
