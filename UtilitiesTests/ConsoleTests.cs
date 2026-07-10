namespace ptr727.Utilities.Tests;

public class ConsoleTests
{
    [Fact]
    public void WriteLineColor_WithValidString_ShouldContainMessage()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineColor(ConsoleColor.Green, "Test message");

            string result = output.ToString();
            _ = result.Should().Contain("Test message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void WriteLineColor_WithEmptyString_ShouldNotThrow()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineColor(ConsoleColor.Green, string.Empty);

            string result = output.ToString();
            _ = result.Should().NotBeNull();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
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
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineError("Error message");

            string result = output.ToString();
            _ = result.Should().Contain("Error message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void WriteLineEvent_WithString_ShouldContainMessage()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineEvent("Event message");

            string result = output.ToString();
            _ = result.Should().Contain("Event message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void WriteLineTool_WithString_ShouldContainMessage()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineTool("Tool message");

            string result = output.ToString();
            _ = result.Should().Contain("Tool message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void WriteLine_WithString_ShouldContainMessage()
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLine("Output message");

            string result = output.ToString();
            _ = result.Should().Contain("Output message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Theory]
    [InlineData(ConsoleColor.Green)]
    [InlineData(ConsoleColor.Red)]
    [InlineData(ConsoleColor.Yellow)]
    [InlineData(ConsoleColor.Cyan)]
    public void WriteLineColor_WithDifferentColors_ShouldNotThrow(ConsoleColor color)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();
        Console.SetOut(output);
        try
        {
            ConsoleEx.WriteLineColor(color, "Test");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
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
