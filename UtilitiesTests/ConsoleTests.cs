using Xunit;

namespace InsaneGenius.Utilities.Tests;

// TODO

public class ConsoleTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public void Null() => Assert.False(false);
}
