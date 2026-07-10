namespace ptr727.Utilities.Tests;

public class ConsoleTests
{
    [Fact]
    public void WriteLineColor_WithValidString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineColor(ConsoleColor.Green, "Test message");

        string result = output.ToString();
        _ = result.Should().Contain("Test message");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLineColor_WithEmptyString_ShouldNotThrow()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineColor(ConsoleColor.Green, string.Empty);

        string result = output.ToString();
        _ = result.Should().NotBeNull();

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLineColor_WithNullObject_ShouldThrowArgumentNullException()
    {
        object? nullValue = null;

        _ = FluentActions
            .Invoking(() => ConsoleEx.WriteLineColor(ConsoleColor.Red, nullValue!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void WriteLineError_WithString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineError("Error message");

        string result = output.ToString();
        _ = result.Should().Contain("Error message");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLineEvent_WithString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineEvent("Event message");

        string result = output.ToString();
        _ = result.Should().Contain("Event message");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLineTool_WithString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineTool("Tool message");

        string result = output.ToString();
        _ = result.Should().Contain("Tool message");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLine_WithString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLine("Output message");

        string result = output.ToString();
        _ = result.Should().Contain("Output message");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Theory]
    [InlineData(ConsoleColor.Green)]
    [InlineData(ConsoleColor.Red)]
    [InlineData(ConsoleColor.Yellow)]
    [InlineData(ConsoleColor.Cyan)]
    public void WriteLineColor_WithDifferentColors_ShouldNotThrow(ConsoleColor color)
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineColor(color, "Test");

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void ColorConstants_ShouldHaveCorrectValues()
    {
        _ = ConsoleEx.ToolColor.Should().Be(ConsoleColor.Green);
        _ = ConsoleEx.ErrorColor.Should().Be(ConsoleColor.Red);
        _ = ConsoleEx.OutputColor.Should().Be(ConsoleColor.Cyan);
        _ = ConsoleEx.EventColor.Should().Be(ConsoleColor.Yellow);
    }
}
