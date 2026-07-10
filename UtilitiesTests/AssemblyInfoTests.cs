using System.Net.Http.Headers;

namespace ptr727.Utilities.Tests;

public class AssemblyInfoTests
{
    [Fact]
    public void For_WithMarkerType_ReturnsDefiningAssembly()
    {
        _ = AssemblyInfo.For<HttpClientOptions>().Should().BeSameAs(typeof(AssemblyInfo).Assembly);
        _ = AssemblyInfo
            .For<AssemblyInfoTests>()
            .Should()
            .BeSameAs(typeof(AssemblyInfoTests).Assembly);
    }

    [Fact]
    public void AppName_IsNotNullOrEmpty() => _ = AssemblyInfo.AppName.Should().NotBeNullOrEmpty();

    [Fact]
    public void AppVersion_IsNotNullOrEmpty() =>
        _ = AssemblyInfo.AppVersion.Should().NotBeNullOrEmpty();

    [Fact]
    public void DefaultUserAgent_ParsesToAppIdentity()
    {
        ProductInfoHeaderValue userAgent = AssemblyInfo.DefaultUserAgent;

        _ = userAgent.Should().NotBeNull();
        _ = userAgent.ToString().Should().NotBeNullOrEmpty();
    }
}
