using System.Net.Http.Headers;

namespace ptr727.Utilities.Tests;

public class HttpClientFactoryTests
{
    [Fact]
    public void GetClient_ReturnsSharedSingleton()
    {
        HttpClient first = HttpClientFactory.GetClient();
        HttpClient second = HttpClientFactory.GetClient();

        _ = first.Should().NotBeNull();
        _ = first.Should().BeSameAs(second);
    }

    [Fact]
    public void CreateClient_WithDefaults_ReturnsIndependentClients()
    {
        using HttpClient first = HttpClientFactory.CreateClient();
        using HttpClient second = HttpClientFactory.CreateClient();

        _ = first.Should().NotBeNull();
        _ = first.Should().NotBeSameAs(second);
    }

    [Fact]
    public void CreateClient_WithDefaults_UsesDefaultUserAgent()
    {
        using HttpClient client = HttpClientFactory.CreateClient();

        _ = client.DefaultRequestHeaders.UserAgent.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateClient_WithUserAgent_AppliesUserAgent()
    {
        ProductInfoHeaderValue userAgent = new("Sample", "9.9");

        using HttpClient client = HttpClientFactory.CreateClient(userAgent);

        _ = client.DefaultRequestHeaders.UserAgent.Should().Contain(userAgent);
    }

    [Fact]
    public void CreateClient_WithNullUserAgent_Throws()
    {
        ProductInfoHeaderValue? userAgent = null;

        _ = FluentActions
            .Invoking(() => HttpClientFactory.CreateClient(userAgent!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateClient_WithOptions_AppliesTimeoutAndBaseAddress()
    {
        HttpClientOptions options = new()
        {
            Timeout = TimeSpan.FromSeconds(42),
            BaseAddress = new Uri("https://example.com/"),
        };

        using HttpClient client = HttpClientFactory.CreateClient(options);

        _ = client.Timeout.Should().Be(TimeSpan.FromSeconds(42));
        _ = client.BaseAddress.Should().Be(options.BaseAddress);
    }

    [Fact]
    public void CreateResilienceHandler_ReturnsHandler()
    {
        using DelegatingHandler handler = HttpClientFactory.CreateResilienceHandler();

        _ = handler.Should().NotBeNull();
    }
}
