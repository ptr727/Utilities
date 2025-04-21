using System;

namespace InsaneGenius.Utilities.Tests;

public class UtilitiesTests : IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}
