using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using InsaneGenius.Utilities;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Reflection;
using System.IO;
using System.IO.Pipelines;
using System.Diagnostics;
using System.Text;
using System.Buffers;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace Sandbox
{
    internal class Program
    {
        // https://github.com/reactiveui/refit
        // https://github.com/loresoft/FluentRest
        // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
        // https://github.com/App-vNext/Polly
        // https://restclient.dalsoft.io/
        // https://flurl.dev/
        // http://restsharp.org/
        // https://github.com/octokit/octokit.net
        // https://github.com/restsharp/RestSharp/issues/735
        // https://github.com/rfrancotechnologies/reliable-rest-client-wrapper
        // https://jeremylindsayni.wordpress.com/2019/01/01/using-polly-and-flurl-to-improve-your-website/


        private static readonly HttpClient GlobalHttpClient = new HttpClient();

        private static async Task<int> Main(string[] args)
        {
            //Uri uri = new Uri(@"https://api.github.com/repos/handbrake/handbrake/releases/latest");
            //Download.DownloadString(uri, out string value);

            //Uri uri = new Uri(@"https://github.com/HandBrake/HandBrake/releases/download/1.3.2/HandBrakeCLI-1.3.2-win-x86_64.zip");
            //Download.GetContentInfo(uri, out long size, out DateTime time);
            //Download.DownloadFile(uri, @"D:\Temp\foo.tmp");

            //UnlistPreReleaseNuGetPackages();

            //const string userId = "ptr727";
            //var repositories = await GetGitHubRepositoriesAsync(userId);

            // Start FfProbe process
            //using FfProbePipeProcess ffprobeProcess = new FfProbePipeProcess();
            //using FfProbeProcess ffprobeProcess = new FfProbeProcess();
            //ffprobeProcess.Run();

            FileEx.DeleteFile("foo");
            FileEx.Options.RetryWaitForCancel();

            //TestSerilog();
            TestSerilog1();

            return 0;
        }

        private static async Task<IEnumerable<NuGetVersion>> GetNuGetPackageVersionsAsync(string packageId)
        {
            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            return await resource.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, CancellationToken.None);
        }

        private static async Task<IEnumerable<object>> GetGitHubRepositoriesAsync(string userId)
        {
            GlobalHttpClient.DefaultRequestHeaders.Accept.Clear();
            GlobalHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.nebula-preview+json"));
            GlobalHttpClient.DefaultRequestHeaders.Add("User-Agent", Assembly.GetExecutingAssembly().GetName().Name);

            Task<Stream> streamTask = GlobalHttpClient.GetStreamAsync($"https://api.github.com/users/{userId}/repos");
            return await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<object>>(await streamTask);
        }

        private static async Task<HttpResponseMessage> NuGetUnlistVersionAsync(string apiKey, string packageId, string packageVersion)
        {
            GlobalHttpClient.DefaultRequestHeaders.Clear();
            GlobalHttpClient.DefaultRequestHeaders.Add("User-Agent", Assembly.GetExecutingAssembly().GetName().Name);
            GlobalHttpClient.DefaultRequestHeaders.Add("X-NuGet-ApiKey", apiKey);
            return await GlobalHttpClient.DeleteAsync($"https://www.nuget.org/api/v2/package/{packageId}/{packageVersion}");

            // Alternative:
            // PackageUpdateResource update = await repository.GetResourceAsync<PackageUpdateResource>();
        }

        private static async void UnlistPreReleaseNuGetPackages()
        {
            // https://www.nuget.org/packages/InsaneGenius.Utilities/
            const string packageId = "InsaneGenius.Utilities";
            IEnumerable<NuGetVersion> packages = await GetNuGetPackageVersionsAsync(packageId);
            foreach (NuGetVersion version in packages)
            {
                Console.WriteLine($"Found version : {version}");

                const string apiKey = "{secret}";
                if (version.ToFullString().Contains("alpha", StringComparison.OrdinalIgnoreCase) ||
                    version.ToFullString().Contains("pullrequest", StringComparison.OrdinalIgnoreCase) ||
                    version.ToFullString().StartsWith("1.0.", StringComparison.OrdinalIgnoreCase) ||
                    version.ToFullString().StartsWith("1.1.", StringComparison.OrdinalIgnoreCase) ||
                    version.ToFullString().StartsWith("1.2.", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Unlisting version {version}");
                    HttpResponseMessage result = await NuGetUnlistVersionAsync(apiKey, packageId, version.ToFullString());
                    if (!result.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to unlist version {version}");
                    }
                }
            }
        }

        // https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/
        public class FfProbePipeProcess : ProcessEx 
        {
            public bool Run()
            {
                // Start FfProbe process
                const string ffProbe = @"C:\Users\piete\source\repos\PlexCleaner\PlexCleaner\bin\Debug\netcoreapp3.1\Tools\FFMpeg\bin\ffprobe.exe";
                const string targetFile = @"D:\Troublesome\Verify - Treasure Island with Bear Grylls - S01E06 - Surviving the Island.mkv";
                string cmdLine = $"-loglevel error -show_packets -of json \"{targetFile}\"";
                RedirectOutput = true;
                RedirectError = true;
                StartEx(ffProbe, cmdLine);

                // Start reading the console output
                Task reading = ReadAsync();

                // Wait for reading and writing to finish
                reading.Wait();
                WaitForExit();

                return true;
            }

            protected override void OutputHandler(DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                // Write to writer pipe
                Pipe.Writer.WriteAsync(Encoding.UTF8.GetBytes(e.Data));
            }

            protected override void ExitHandler(EventArgs e)
            {
                // Signal writer pipe that we are done
                Pipe.Writer.Complete();

                // Call base
                base.ExitHandler(e);
            }

            private async Task ReadAsync()
            {
                while (true)
                {
                    ReadResult result = await Pipe.Reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = result.Buffer;
                    SequencePosition? position = null;

                    do
                    {
                        // Look for a } in the buffer
                        // Encoding.UTF8.GetBytes("}")
                        position = buffer.PositionOf((byte)'}');
                        if (position != null)
                        {
                            // Process the line
                            //ProcessLine(buffer.Slice(0, position.Value));
                            // Avoid .ToArray()
                            string block = Encoding.UTF8.GetString(buffer.Slice(0, position.Value).ToArray());

                            // Skip the line + the \n character (basically position)
                            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                        }
                    }
                    while (position != null);

                    // Tell the PipeReader how much of the buffer we have consumed
                    Pipe.Reader.AdvanceTo(buffer.Start, buffer.End);

                    // Stop reading if there's no more data coming
                    if (result.IsCompleted)
                    {
                        break;
                    }
                }

                // Mark the PipeReader as complete
                Pipe.Reader.Complete();
            }

            private Pipe Pipe = new Pipe();
        }

        public class FfProbeProcess : ProcessEx
        {
            public bool Run()
            {
                // Start FfProbe process
                const string ffProbe = @"C:\Users\piete\source\repos\PlexCleaner\PlexCleaner\bin\Debug\netcoreapp3.1\Tools\FFMpeg\bin\ffprobe.exe";
                const string targetFile = @"D:\Troublesome\Verify - Treasure Island with Bear Grylls - S01E06 - Surviving the Island.mkv";
                string cmdLine = $"-loglevel error -show_packets -of json \"{targetFile}\"";
                RedirectOutput = true;
                RedirectError = true;
                StartEx(ffProbe, cmdLine);

                // Wait for exit
                int result = WaitForExitAsync().Result;

                return true;
            }

            protected override void OutputHandler(DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;
                
                // Well formed output
                /*
                {
                    "packets": [
                        {
                            "codec_type": "video",
                            "stream_index": 0,
                            "pts": 80,
                            "pts_time": "0.080000",
                            "duration": 40,
                            "duration_time": "0.040000",
                            "size": "105942",
                            "pos": "1098",
                            "flags": "K_"
                        },
                        {
                            "codec_type": "audio",
                            "stream_index": 1,
                            "pts": 0,
                            "pts_time": "0.000000",
                            "dts": 0,
                            "dts_time": "0.000000",
                            "duration": 21,
                            "duration_time": "0.021000",
                            "size": "351",
                            "pos": "109853",
                            "flags": "K_"
                        }
                    ]
                }
                */

                // Look for header
                if (!JsonHeaderFound)
                {
                    string header = e.Data.Trim();
                    if (header.Equals("\"packets\": ["))
                        JsonHeaderFound = true;
                    return;
                }

                JsonBuffer.AppendLine(e.Data);
                // TODO: Not very efficient
                string json = JsonBuffer.ToString().Trim();
                if (json.StartsWith("{") && 
                    (json.EndsWith("}") || json.EndsWith("},")))
                { 
                    JsonBuffer.Clear();
                    object foo = System.Text.Json.JsonSerializer.Deserialize<object>(json.TrimEnd(','));
                }

                // Look for footer
                if (!JsonFooterFound)
                {
                    string footer = e.Data.Trim();
                    if (footer.Equals("]"))
                        JsonFooterFound = true;
                    return;
                }
            }

            private readonly StringBuilder JsonBuffer = new StringBuilder();
            private bool JsonHeaderFound = false;
            private bool JsonFooterFound = false;
        }

        private static void TestSerilog()
        {
            // How to create a "Microsoft.Extensions.Logging.ILogger" from a "Serilog.ILogger"?
            Microsoft.Extensions.Logging.ILogger logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            // https://github.com/serilog/serilog-extensions-logging/blob/dev/samples/Sample/Program.cs
            LoggerProviderCollection providerCollection = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Providers(providerCollection)
                .CreateLogger();

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(providerCollection);
            serviceCollection.AddSingleton<ILoggerFactory>(sc =>
            {
                LoggerProviderCollection loggerProviderCollection = sc.GetService<LoggerProviderCollection>();
                SerilogLoggerFactory serilogLoggerFactory = new SerilogLoggerFactory(null, true, loggerProviderCollection);

                foreach (ILoggerProvider loggerProvider in sc.GetServices<ILoggerProvider>())
                    serilogLoggerFactory.AddProvider(loggerProvider);

                return serilogLoggerFactory;
            });

            serviceCollection.AddLogging(logging => logging.AddConsole());

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger<Program> programLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger = programLogger;
            logger.LogInformation("Testing testing");
        }

        private static void TestSerilog1()
        {
            // https://www.nexmo.com/legacy-blog/2020/02/10/adaptive-library-logging-with-microsoft-extensions-logging-dr

            // Default : "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            // "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} {SourceContext} | {Message:lj}{NewLine}{Exception}"

            // Serilog logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:O} {Level} {SourceContext} | {Message}{NewLine}{Exception}", 
                                 theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            // MEL logger factory
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog(Log.Logger);

            // MEL logger
            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger("Foo");
            logger.LogInformation("Testing testing");

            // IG logger
            LogOptions.CreateLogger(loggerFactory);
            LogOptions.Logger.LogInformation("Testing testing");
            LogOptions.Logger.LogWarning("Testing testing");
            LogOptions.Logger.LogError("Testing testing");
        }
    }
}
