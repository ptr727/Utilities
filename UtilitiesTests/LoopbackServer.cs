using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ptr727.Utilities.Tests;

/// <summary>
/// A minimal HTTP server bound to the loopback interface, so the <see cref="Download"/> tests
/// exercise success, failure, and cancellation without reaching the network or depending on a
/// third party staying reachable.
/// </summary>
/// <remarks>
/// Each instance binds its own ephemeral port, so tests running in parallel do not share routes or
/// state. Disposing the server stops the listener and completes any request still in flight.
/// </remarks>
internal sealed class LoopbackServer : IDisposable
{
    /// <summary>
    /// The body every successful route returns.
    /// </summary>
    public const string Content = "loopback content";

    /// <summary>
    /// How long the slow route holds a request before responding, long enough that a caller
    /// cancelling after a short delay wins the race deterministically.
    /// </summary>
    public static readonly TimeSpan SlowResponseDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopbackServer"/> class and starts listening.
    /// </summary>
    public LoopbackServer()
    {
        // HttpListener takes a prefix rather than a socket, so a free port is probed and released first.
        BaseAddress = new Uri(
            FormattableString.Invariant($"http://127.0.0.1:{GetEphemeralPort()}/")
        );
        _listener.Prefixes.Add(BaseAddress.AbsoluteUri);
        _listener.Start();
        _listenTask = Task.Run(ListenAsync);
    }

    /// <summary>
    /// Gets the root address the server is listening on.
    /// </summary>
    public Uri BaseAddress { get; }

    /// <summary>
    /// Gets the address of the route that responds 200 with <see cref="Content"/>.
    /// </summary>
    public Uri OkUri => new(BaseAddress, OkPath);

    /// <summary>
    /// Gets the address of the route that responds 404.
    /// </summary>
    public Uri MissingUri => new(BaseAddress, MissingPath);

    /// <summary>
    /// Gets the address of the route that waits <see cref="SlowResponseDelay"/> before responding.
    /// </summary>
    public Uri SlowUri => new(BaseAddress, SlowPath);

    /// <summary>
    /// Stops the listener and waits for the accept loop to finish.
    /// </summary>
    public void Dispose()
    {
        _cancellation.Cancel();
        _listener.Close();

        // The accept loop swallows its own failures, so a stopped listener is not a teardown failure.
        _listenTask.GetAwaiter().GetResult();

        _cancellation.Dispose();
    }

    private static int GetEphemeralPort()
    {
        using TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    private async Task ListenAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                // The listener was closed by Dispose while waiting for a request.
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            await RespondAsync(context).ConfigureAwait(false);
        }
    }

    private async Task RespondAsync(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;
        try
        {
            switch (context.Request.Url?.AbsolutePath)
            {
                case "/" + OkPath:
                    await WriteContentAsync(response).ConfigureAwait(false);
                    break;

                case "/" + SlowPath:
                    await Task.Delay(SlowResponseDelay, _cancellation.Token).ConfigureAwait(false);
                    await WriteContentAsync(response).ConfigureAwait(false);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Dispose ran, or the client went away, while the slow route was waiting.
        }
        catch (HttpListenerException)
        {
            // The client disconnected before the response was written, as the cancellation test does.
        }
        catch (IOException)
        {
            // Same disconnect, surfaced from the response stream rather than the listener.
        }
        finally
        {
            Close(response);
        }
    }

    private static async Task WriteContentAsync(HttpListenerResponse response)
    {
        byte[] body = Encoding.UTF8.GetBytes(Content);

        // GetContentInfo reports Content-Length as the size, so it is set rather than left to chunking.
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = body.Length;
        response.Headers.Set("Last-Modified", s_lastModified.ToString("R", null));

        await response.OutputStream.WriteAsync(body).ConfigureAwait(false);
    }

    private static void Close(HttpListenerResponse response)
    {
        try
        {
            response.Close();
        }
        catch (HttpListenerException)
        {
            // Closing a response whose client already disconnected is not a failure.
        }
        catch (ObjectDisposedException)
        {
            // The listener was disposed first.
        }
    }

    private const string OkPath = "ok";
    private const string MissingPath = "missing";
    private const string SlowPath = "slow";

    private static readonly DateTimeOffset s_lastModified = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task _listenTask;
}
