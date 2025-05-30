using System;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class DownloadTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public void GetUriInformation()
    {
        Uri uri = new(
            "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
        );
        Assert.True(Download.GetContentInfo(uri, out long _, out DateTime _));
    }
}
