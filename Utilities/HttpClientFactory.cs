using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ptr727.Utilities;

/// <summary>
/// Creates resilient <see cref="HttpClient"/> instances with retry and circuit-breaker
/// policies, connection pooling, and an AOT safe default User-Agent.
/// </summary>
/// <remarks>
/// Use <see cref="GetClient"/> for a shared singleton, <see cref="CreateClient(HttpClientOptions)"/>
/// to build a caller-owned client from tuned options, or <see cref="CreateResilienceHandler"/> to
/// obtain the resilience handler and build a client with a custom base address or headers.
/// </remarks>
public static class HttpClientFactory
{
    private static readonly Lazy<ILogger> s_logger = new(() =>
        LogOptions.CreateLogger(typeof(HttpClientFactory).FullName!)
    );

    private static ILogger Log => s_logger.Value;

    private static readonly HttpClientOptions s_defaultOptions = new();

    private static readonly Lazy<HttpClient> s_httpClient = new(() =>
        CreateClient(s_defaultOptions)
    );

    /// <summary>
    /// Gets the shared, lazily-initialized <see cref="HttpClient"/> configured with default options.
    /// </summary>
    /// <returns>The shared HttpClient instance.</returns>
    /// <remarks>
    /// All callers share this client, its connection pool, and its circuit-breaker state. Do not
    /// dispose it; it lives for the process lifetime. Use <see cref="CreateClient(HttpClientOptions)"/>
    /// for a caller-owned instance.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1024:Use properties where appropriate",
        Justification = "Exposed as a method so callers see they receive the shared, lazily-initialized HttpClient rather than a lightweight property value."
    )]
    public static HttpClient GetClient() => s_httpClient.Value;

    /// <summary>
    /// Creates a new caller-owned <see cref="HttpClient"/> from the specified options.
    /// </summary>
    /// <param name="options">The options to configure the client. When null, defaults are used.</param>
    /// <returns>A configured HttpClient. The caller owns and should store and reuse the instance.</returns>
    /// <remarks>
    /// Each client gets an independent resilience handler and circuit-breaker state. Disposing the
    /// client disposes the whole handler chain.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "HttpClient takes ownership of the handler and disposes it when the client is disposed."
    )]
    public static HttpClient CreateClient(HttpClientOptions? options = null)
    {
        options ??= s_defaultOptions;

        HttpClient client = new(CreateResilienceHandler(options)) { Timeout = options.Timeout };
        if (options.BaseAddress is not null)
        {
            client.BaseAddress = options.BaseAddress;
        }

        client.DefaultRequestHeaders.UserAgent.Add(
            options.UserAgent ?? AssemblyInfo.DefaultUserAgent
        );

        return client;
    }

    /// <summary>
    /// Creates the resilience handler (retry, circuit breaker, connection pooling) for callers
    /// who build their own <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="options">The options to configure the handler. When null, defaults are used.</param>
    /// <returns>
    /// The resilience handler. Ownership transfers to the <see cref="HttpClient"/> it is passed to,
    /// which disposes the handler chain when the client is disposed.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The returned handler owns the inner SocketsHttpHandler; the consuming HttpClient disposes the chain."
    )]
    public static DelegatingHandler CreateResilienceHandler(HttpClientOptions? options = null)
    {
        options ??= s_defaultOptions;

        ResiliencePipeline<HttpResponseMessage> pipeline =
            new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(
                    new RetryStrategyOptions<HttpResponseMessage>
                    {
                        MaxRetryAttempts = options.RetryMaxAttempts,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = options.RetryBaseDelay,
                        MaxDelay = options.RetryMaxDelay,
                        ShouldHandle = args =>
                            ValueTask.FromResult(IsTransientFailure(args.Outcome)),
                        OnRetry = args =>
                        {
                            Log.LogHttpRetry(
                                args.AttemptNumber,
                                args.RetryDelay.TotalMilliseconds,
                                FormatOutcome(args.Outcome)
                            );
                            return ValueTask.CompletedTask;
                        },
                    }
                )
                .AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions<HttpResponseMessage>
                    {
                        FailureRatio = options.CircuitBreakerFailureRatio,
                        MinimumThroughput = options.CircuitBreakerMinimumThroughput,
                        SamplingDuration = options.CircuitBreakerSamplingDuration,
                        BreakDuration = options.CircuitBreakerBreakDuration,
                        ShouldHandle = args =>
                            ValueTask.FromResult(IsTransientFailure(args.Outcome)),
                        OnOpened = args =>
                        {
                            Log.LogCircuitOpened(
                                args.BreakDuration.TotalSeconds,
                                FormatOutcome(args.Outcome)
                            );
                            return ValueTask.CompletedTask;
                        },
                        OnClosed = _ =>
                        {
                            Log.LogCircuitClosed();
                            return ValueTask.CompletedTask;
                        },
                        OnHalfOpened = _ =>
                        {
                            Log.LogCircuitHalfOpened();
                            return ValueTask.CompletedTask;
                        },
                    }
                )
                .Build();

        return new ResilienceHandler(pipeline)
        {
            InnerHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = options.PooledConnectionLifetime,
                PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout,
                AutomaticDecompression = options.AutomaticDecompression,
            },
        };
    }

    private static bool IsTransientFailure(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is null)
        {
            return outcome.Result is not null
                && (int)outcome.Result.StatusCode is 408 or 429 or >= 500;
        }

        // Retry known-transient failures: a request timeout (a cancellation with an inner
        // TimeoutException), a network or IO error (an HttpRequestException with no status, or an
        // IOException), or an HttpRequestException whose own status code is transient (408, 429,
        // >= 500). Caller cancellation, an open circuit, a 4xx status, and any other exception
        // (including programming errors) are not retried.
        return outcome.Exception switch
        {
            OperationCanceledException canceled => canceled.InnerException is TimeoutException,
            HttpRequestException { StatusCode: null } or IOException => true,
            HttpRequestException { StatusCode: { } statusCode } => (int)statusCode
                is 408
                    or 429
                    or >= 500,
            _ => false,
        };
    }

    private static string FormatOutcome(Outcome<HttpResponseMessage> outcome) =>
        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "unknown";
}
