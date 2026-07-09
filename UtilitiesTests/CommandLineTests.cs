using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class CommandLineTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public void ParseArguments()
    {
        const string input =
            @"/src:""C:\tmp\Some Folder\Sub Folder"" /users:""abcdefg@hijkl.com"" tasks:""SomeTask,Some Other Task"" -someParam foo D:\";
        string[] expected =
        [
            @"/src:""C:\tmp\Some Folder\Sub Folder""",
            @"/users:""abcdefg@hijkl.com""",
            @"tasks:""SomeTask,Some Other Task""",
            @"-someParam",
            @"foo",
            @"D:\",
        ];

        string[] output = CommandLineEx.ParseArguments(input);
        _ = output.Should().Equal(expected);
    }

    [Fact]
    public void GetCommandLineArgs()
    {
        string[] commandlineArgs = CommandLineEx.GetCommandLineArgs();
        _ = (commandlineArgs.Length > 0).Should().BeTrue();
    }
}
