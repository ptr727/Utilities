using System.Net;
using System.Runtime.CompilerServices;

namespace ptr727.Utilities.Tests;

/// <summary>
/// Process-wide HTTP defaults for the test assembly.
/// </summary>
internal static class TestHttpDefaults
{
    /// <summary>
    /// Disables the ambient proxy for every client the tests build.
    /// </summary>
    /// <remarks>
    /// <see cref="HttpClientFactory"/> leaves <c>SocketsHttpHandler.UseProxy</c> at its default, so
    /// a client resolves <see cref="HttpClient.DefaultProxy"/>, which on Unix is read from
    /// <c>http_proxy</c> and its siblings and bypasses loopback only where <c>no_proxy</c> says to.
    /// A developer machine or runner configured that way would send every loopback request in
    /// <see cref="LoopbackServer"/>'s tests to a proxy, which is the environment dependence these
    /// tests exist to remove. A <see cref="WebProxy"/> with no address bypasses everything.
    /// </remarks>
    [ModuleInitializer]
    internal static void DisableAmbientProxy() => HttpClient.DefaultProxy = new WebProxy();
}
