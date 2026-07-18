using System.Net;

namespace ptr727.Utilities.Tests;

public class HttpClientFactoryResilienceTests
{
    [Fact]
    public async Task TransientStatus_IsRetriedThenSucceeds()
    {
        using StubHttpMessageHandler stub = new(
            Status(HttpStatusCode.InternalServerError),
            Status(HttpStatusCode.ServiceUnavailable),
            Status(HttpStatusCode.OK)
        );
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("http://localhost/"),
            TestContext.Current.CancellationToken
        );

        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        _ = stub.CallCount.Should().Be(3); // initial attempt + 2 retries
    }

    [Fact]
    public async Task NonTransientStatus_IsNotRetried()
    {
        using StubHttpMessageHandler stub = new(Status(HttpStatusCode.NotFound));
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("http://localhost/"),
            TestContext.Current.CancellationToken
        );

        _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _ = stub.CallCount.Should().Be(1); // 404 is not transient
    }

    [Fact]
    public async Task Timeout_IsRetried()
    {
        // A request timeout surfaces as a cancellation carrying an inner TimeoutException.
        using StubHttpMessageHandler stub = new(
            Throws(new TaskCanceledException("timeout", new TimeoutException()))
        );
        HttpClientOptions options = FastOptions();
        options.RetryMaxAttempts = 2;
        using HttpClient client = CreateStubbedClient(stub, options);

        _ = await FluentActions
            .Awaiting(() => client.GetAsync(new Uri("http://localhost/")))
            .Should()
            .ThrowAsync<TaskCanceledException>();
        _ = stub.CallCount.Should().Be(3); // initial attempt + 2 retries
    }

    [Fact]
    public async Task CallerCancellation_IsNotRetried()
    {
        // Caller-requested cancellation is not transient and must not be retried.
        using StubHttpMessageHandler stub = new(
            Throws(new OperationCanceledException("cancelled"))
        );
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        _ = await FluentActions
            .Awaiting(() => client.GetAsync(new Uri("http://localhost/")))
            .Should()
            .ThrowAsync<OperationCanceledException>();
        _ = stub.CallCount.Should().Be(1); // no retry on cancellation
    }

    [Fact]
    public async Task NetworkException_IsRetried()
    {
        using StubHttpMessageHandler stub = new(Throws(new HttpRequestException("network")));
        HttpClientOptions options = FastOptions();
        options.RetryMaxAttempts = 2;
        using HttpClient client = CreateStubbedClient(stub, options);

        _ = await FluentActions
            .Awaiting(() => client.GetAsync(new Uri("http://localhost/")))
            .Should()
            .ThrowAsync<HttpRequestException>();
        _ = stub.CallCount.Should().Be(3); // initial attempt + 2 retries
    }

    [Fact]
    public async Task NonTransientException_IsNotRetried()
    {
        using StubHttpMessageHandler stub = new(Throws(new InvalidOperationException("bug")));
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        _ = await FluentActions
            .Awaiting(() => client.GetAsync(new Uri("http://localhost/")))
            .Should()
            .ThrowAsync<InvalidOperationException>();
        _ = stub.CallCount.Should().Be(1); // programming errors are not retried
    }

    [Fact]
    public async Task HttpRequestExceptionWithNonTransientStatus_IsNotRetried()
    {
        using StubHttpMessageHandler stub = new(
            Throws(new HttpRequestException("not found", null, HttpStatusCode.NotFound))
        );
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        _ = await FluentActions
            .Awaiting(() => client.GetAsync(new Uri("http://localhost/")))
            .Should()
            .ThrowAsync<HttpRequestException>();
        _ = stub.CallCount.Should().Be(1); // a 404 HttpRequestException is not transient
    }

    [Fact]
    public async Task HttpRequestExceptionWithTransientStatus_IsRetriedThenSucceeds()
    {
        using StubHttpMessageHandler stub = new(
            Throws(
                new HttpRequestException("unavailable", null, HttpStatusCode.ServiceUnavailable)
            ),
            Throws(
                new HttpRequestException("unavailable", null, HttpStatusCode.ServiceUnavailable)
            ),
            Status(HttpStatusCode.OK)
        );
        using HttpClient client = CreateStubbedClient(stub, FastOptions());

        using HttpResponseMessage response = await client.GetAsync(
            new Uri("http://localhost/"),
            TestContext.Current.CancellationToken
        );

        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        _ = stub.CallCount.Should().Be(3); // 503 exception retried, then success
    }

    private static HttpClientOptions FastOptions() =>
        new()
        {
            RetryBaseDelay = TimeSpan.FromMilliseconds(1),
            RetryMaxDelay = TimeSpan.FromMilliseconds(5),
        };

    private static Func<Task<HttpResponseMessage>> Status(HttpStatusCode statusCode) =>
        () => Task.FromResult(new HttpResponseMessage(statusCode));

    private static Func<Task<HttpResponseMessage>> Throws(Exception exception) =>
        () => Task.FromException<HttpResponseMessage>(exception);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership of the handler chain transfers to the returned HttpClient, which the caller disposes."
    )]
    private static HttpClient CreateStubbedClient(
        StubHttpMessageHandler stub,
        HttpClientOptions options
    )
    {
        DelegatingHandler resilience = HttpClientFactory.CreateResilienceHandler(options);

        // Replace the factory's real network handler with the stub transport.
        resilience.InnerHandler?.Dispose();
        resilience.InnerHandler = stub;

        return new HttpClient(resilience);
    }
}

/// <summary>
/// A stub HTTP transport that returns a scripted sequence of results and counts calls. The last
/// step repeats once the sequence is exhausted, so a single step models a repeating outcome.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<Task<HttpResponseMessage>>> _steps;

    internal StubHttpMessageHandler(params Func<Task<HttpResponseMessage>>[] steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        if (steps.Length == 0)
        {
            throw new ArgumentException("At least one step is required.", nameof(steps));
        }

        _steps = new Queue<Func<Task<HttpResponseMessage>>>(steps);
    }

    internal int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        CallCount++;
        return (_steps.Count > 1 ? _steps.Dequeue() : _steps.Peek())();
    }
}
