namespace ptr727.Utilities.Sandbox;

/// <summary>
/// Sandbox entry point. Each sample class exercises a slice of the library and is invoked
/// from <see cref="Main"/>; comment out a call to skip that sample for a given run.
/// </summary>
internal sealed class Program
{
    private Program() { }

    private static async Task<int> Main()
    {
        // Configure logging: build a Serilog console logger and inject it into the library.
        Log.Logger = LoggerFactory.Create();
        LogOptions.SetFactory(LoggerFactory.CreateLoggerFactory());

        try
        {
            if (!AssemblyIdentitySample.Run())
            {
                return 1;
            }

            await HttpClientSample.RunAsync().ConfigureAwait(false);

            return 0;
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }
}
