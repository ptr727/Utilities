using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using InsaneGenius.Utilities;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Web;
using System.Reflection;

namespace Sandbox
{
    class Program
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

        static async Task Main(string[] args)
        {
            //Uri uri = new Uri(@"https://api.github.com/repos/handbrake/handbrake/releases/latest");
            //Download.DownloadString(uri, out string value);

            //Uri uri = new Uri(@"https://github.com/HandBrake/HandBrake/releases/download/1.3.2/HandBrakeCLI-1.3.2-win-x86_64.zip");
            //Download.GetContentInfo(uri, out long size, out DateTime time);
            //Download.DownloadFile(uri, @"D:\Temp\foo.tmp");

            //UnlistPreReleaseNuGetPackages();

            //const string userId = "ptr727";
            //var repositories = await GetGitHubRepositoriesAsync(userId);
        }

        public static async Task<IEnumerable<NuGetVersion>> GetNuGetPackageVersionsAsync(string packageId)
        {
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            return await resource.GetAllVersionsAsync(packageId, cache, logger, cancellationToken);
        }

        private static async Task<IEnumerable<object>> GetGitHubRepositoriesAsync(string userId)
        {
            GlobalHttpClient.DefaultRequestHeaders.Accept.Clear();
            GlobalHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.nebula-preview+json"));
            GlobalHttpClient.DefaultRequestHeaders.Add("User-Agent", Assembly.GetExecutingAssembly().GetName().Name);

            var streamTask = GlobalHttpClient.GetStreamAsync($"https://api.github.com/users/{userId}/repos");
            return await JsonSerializer.DeserializeAsync<IEnumerable<object>>(await streamTask);
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
            var packages = await GetNuGetPackageVersionsAsync(packageId);
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
                    var result = await NuGetUnlistVersionAsync(apiKey, packageId, version.ToFullString());
                    if (!result.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to unlist version {version}");
                    }
                }
            }
        }
    }
}
