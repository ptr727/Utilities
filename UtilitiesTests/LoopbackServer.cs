using System.Collections.Concurrent;
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
/// The responses are written straight onto a TCP socket rather than through
/// <see cref="HttpListener"/>, which on Windows resolves an explicit-address prefix through
/// http.sys and needs a URL reservation an unelevated developer does not have. Each instance binds
/// its own ephemeral port, so tests running in parallel share no routes or state.
/// </remarks>
internal sealed class LoopbackServer : IDisposable
{
    /// <summary>
    /// The body every successful route returns.
    /// </summary>
    public const string Content = "loopback content";

    /// <summary>
    /// How long the slow route holds a request before responding, far longer than any test waits.
    /// </summary>
    public static readonly TimeSpan SlowResponseDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopbackServer"/> class and starts listening.
    /// </summary>
    public LoopbackServer()
    {
        // Binding port 0 and reading the port back leaves no window for another listener to claim it.
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();

        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        BaseAddress = new Uri(FormattableString.Invariant($"http://127.0.0.1:{port}/"));

        _acceptTask = Task.Run(AcceptAsync);
    }

    /// <summary>
    /// Gets the byte length of <see cref="Content"/> on the wire, which is what a Content-Length
    /// header and <see cref="Download.GetContentInfo"/> report.
    /// </summary>
    public static int ContentLength => Encoding.UTF8.GetByteCount(Content);

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
    /// Gets a task that completes once the slow route has received a request, so a test cancels a
    /// request the server is known to be holding rather than racing it.
    /// </summary>
    public Task SlowRequestStarted => _slowRequestStarted.Task;

    /// <summary>
    /// Gets the number of requests the server has routed, so a test asserting a failure can prove
    /// the request reached the route rather than failing before it.
    /// </summary>
    public int RequestCount => Volatile.Read(ref _requestCount);

    /// <summary>
    /// Stops the listener and waits for the accept loop to finish.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cancellation.Cancel();
        _listener.Stop();
        _listener.Dispose();

        // Teardown never fails a test whose assertions already passed.
        // The loops are waited for without rethrowing their results, and each wait is bounded.
        // The accept loop is the only thing that adds a connection.
        // Waiting for it first is what makes the set final rather than a mid-iteration snapshot.
        _ = Task.WhenAny(_acceptTask, Task.Delay(s_disposeTimeout)).GetAwaiter().GetResult();
        _ = _acceptTask.Exception;

        Task served = Task.WhenAll(_connections.Keys);
        _ = Task.WhenAny(served, Task.Delay(s_disposeTimeout)).GetAwaiter().GetResult();
        _ = served.Exception;

        _cancellation.Dispose();
    }

    private async Task AcceptAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener
                    .AcceptTcpClientAsync(_cancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }

            Task connection = Task.Run(async () =>
            {
                using (client)
                {
                    await RespondAsync(client).ConfigureAwait(false);
                }
            });
            _ = _connections.TryAdd(connection, 0);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "A test server's accept loop must not fault on one connection's failure."
    )]
    private async Task RespondAsync(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            string target = await ReadRequestTargetAsync(stream).ConfigureAwait(false);

            // A peer that connects and sends nothing is not a request.
            // Counting it would make the exact-count assertions depend on whether one happened.
            if (target.Length == 0)
            {
                return;
            }

            _ = Interlocked.Increment(ref _requestCount);

            switch (target)
            {
                case "/" + OkPath:
                    await WriteResponseAsync(stream, 200, "OK", Content).ConfigureAwait(false);
                    break;

                case "/" + SlowPath:
                    _ = _slowRequestStarted.TrySetResult();
                    await Task.Delay(SlowResponseDelay, _cancellation.Token).ConfigureAwait(false);
                    await WriteResponseAsync(stream, 200, "OK", Content).ConfigureAwait(false);
                    break;

                // The default arm answers the same way, so this case is what names the route.
                case "/" + MissingPath:
                default:
                    await WriteResponseAsync(stream, 404, "Not Found", string.Empty)
                        .ConfigureAwait(false);
                    break;
            }
        }
        catch (Exception)
        {
            // A mid-response disconnect and a Dispose cancelling the slow route both land here.
        }
    }

    private async Task<string> ReadRequestTargetAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[ReadBufferBytes];
        StringBuilder head = new();

        while (head.Length < MaxRequestHeadBytes)
        {
            int read = await stream.ReadAsync(buffer, _cancellation.Token).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            // The request line and the headers are ASCII, and only the request line is read.
            _ = head.Append(Encoding.ASCII.GetString(buffer, 0, read));
            if (head.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                break;
            }
        }

        string text = head.ToString();
        int lineEnd = text.IndexOf("\r\n", StringComparison.Ordinal);
        string requestLine = lineEnd < 0 ? text : text[..lineEnd];
        string[] parts = requestLine.Split(' ');

        return parts.Length >= 2 ? parts[1] : string.Empty;
    }

    private async Task WriteResponseAsync(
        NetworkStream stream,
        int status,
        string reason,
        string body
    )
    {
        byte[] payload = Encoding.UTF8.GetBytes(body);

        // Content-Length is what GetContentInfo reports as the size, so it is always written.
        string head = string.Join(
            "\r\n",
            FormattableString.Invariant($"HTTP/1.1 {status} {reason}"),
            "Content-Type: text/plain; charset=utf-8",
            FormattableString.Invariant($"Content-Length: {payload.Length}"),
            "Last-Modified: " + LastModified,
            "Connection: close",
            string.Empty,
            string.Empty
        );

        await stream
            .WriteAsync(Encoding.ASCII.GetBytes(head), _cancellation.Token)
            .ConfigureAwait(false);
        await stream.WriteAsync(payload, _cancellation.Token).ConfigureAwait(false);
        await stream.FlushAsync(_cancellation.Token).ConfigureAwait(false);
    }

    private const string OkPath = "ok";
    private const string MissingPath = "missing";
    private const string SlowPath = "slow";
    private const string LastModified = "Wed, 01 Jan 2020 00:00:00 GMT";
    private const int ReadBufferBytes = 2048;
    private const int MaxRequestHeadBytes = 16384;

    private static readonly TimeSpan s_disposeTimeout = TimeSpan.FromSeconds(10);

    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TaskCompletionSource _slowRequestStarted = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );
    private readonly Task _acceptTask;
    private readonly ConcurrentDictionary<Task, byte> _connections = new();
    private int _requestCount;
    private bool _disposed;
}
