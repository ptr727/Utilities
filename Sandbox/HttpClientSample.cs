namespace ptr727.Utilities.Sandbox;

/// <summary>
/// Exercises the resilient <see cref="HttpClientFactory"/> pipeline under the current runtime
/// (AOT or managed). A request to a closed local port forces a deterministic transient failure
/// that drives the retry path offline; the emitted retry log lines are the runtime proof that
/// the Polly pipeline executed.
/// </summary>
internal static class HttpClientSample
{
    /// <summary>
    /// Runs the sample.
    /// </summary>
    /// <returns>A task that completes when the sample finishes.</returns>
    internal static async Task RunAsync()
    {
        HttpClientOptions probe = new()
        {
            Timeout = TimeSpan.FromSeconds(5),
            RetryBaseDelay = TimeSpan.FromMilliseconds(10),
            RetryMaxDelay = TimeSpan.FromMilliseconds(50),
        };
        using HttpClient client = HttpClientFactory.CreateClient(probe);
        try
        {
            using HttpResponseMessage response = await client
                .GetAsync(new Uri("http://127.0.0.1:1/"))
                .ConfigureAwait(false);
            Log.Logger.Information("Unexpected success: {StatusCode}", response.StatusCode);
        }
        catch (HttpRequestException exception)
        {
            Log.Logger.Information(
                "Resilience pipeline exercised, expected failure: {Message}",
                exception.Message
            );
        }
    }
}
