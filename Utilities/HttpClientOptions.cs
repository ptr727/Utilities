using System.Net;
using System.Net.Http.Headers;

namespace ptr727.Utilities;

/// <summary>
/// Configuration options for <see cref="HttpClientFactory"/>, with best-practice defaults.
/// </summary>
/// <remarks>
/// Every option has a sensible default, so a caller overrides only what they need. Defaults
/// are chosen for general-purpose resilient HTTP access and can be tuned per client.
/// </remarks>
public sealed class HttpClientOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient failures. Default is 3.
    /// </summary>
    public int RetryMaxAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retries. Default is 1 second.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay between retries. Default is 30 seconds.
    /// </summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the failure ratio that trips the circuit breaker. Default is 0.1 (10 percent).
    /// </summary>
    public double CircuitBreakerFailureRatio { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the minimum throughput before the circuit breaker can trip. Default is 10.
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Gets or sets the sampling duration over which failures are measured. Default is 60 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the duration the circuit stays open once tripped. Default is 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets how long a pooled connection is reused before being recycled. Default is 15 minutes.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Gets or sets how long an idle pooled connection is kept before being closed. Default is 2 minutes.
    /// </summary>
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the automatic decompression methods. Default is <see cref="DecompressionMethods.All"/>.
    /// </summary>
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;

    /// <summary>
    /// Gets or sets the overall HTTP request timeout. Default is 120 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Gets or sets the User-Agent header value. When null, <see cref="AssemblyInfo.DefaultUserAgent"/> is used.
    /// </summary>
    public ProductInfoHeaderValue? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the base address applied to the created client. When null, no base address is set.
    /// </summary>
    public Uri? BaseAddress { get; set; }
}
