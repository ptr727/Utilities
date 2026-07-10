using System.Net;

namespace ptr727.Utilities.Tests;

public class HttpClientOptionsTests
{
    [Fact]
    public void Defaults_MatchBestPracticeValues()
    {
        HttpClientOptions options = new();

        _ = options.RetryMaxAttempts.Should().Be(3);
        _ = options.RetryBaseDelay.Should().Be(TimeSpan.FromSeconds(1));
        _ = options.RetryMaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        _ = options.CircuitBreakerFailureRatio.Should().Be(0.1);
        _ = options.CircuitBreakerMinimumThroughput.Should().Be(10);
        _ = options.CircuitBreakerSamplingDuration.Should().Be(TimeSpan.FromSeconds(60));
        _ = options.CircuitBreakerBreakDuration.Should().Be(TimeSpan.FromSeconds(30));
        _ = options.PooledConnectionLifetime.Should().Be(TimeSpan.FromMinutes(15));
        _ = options.PooledConnectionIdleTimeout.Should().Be(TimeSpan.FromMinutes(2));
        _ = options.AutomaticDecompression.Should().Be(DecompressionMethods.All);
        _ = options.Timeout.Should().Be(TimeSpan.FromSeconds(120));
        _ = options.UserAgent.Should().BeNull();
        _ = options.BaseAddress.Should().BeNull();
    }
}
