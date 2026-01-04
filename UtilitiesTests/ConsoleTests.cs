using System;
using System.IO;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class ConsoleTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public void WriteLineColor_WithValidString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineColor(ConsoleColor.Green, "Test message");

        string result = output.ToString();
        Assert.Contains("Test message", result);

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
        Assert.NotNull(result);

        // Reset console
        StreamWriter standardOutput = new(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(standardOutput);
    }

    [Fact]
    public void WriteLineColor_WithNullObject_ShouldThrowArgumentNullException()
    {
        object? nullValue = null;

        _ = Assert.Throws<ArgumentNullException>(() =>
            ConsoleEx.WriteLineColor(ConsoleColor.Red, nullValue!)
        );
    }

    [Fact]
    public void WriteLineError_WithString_ShouldContainMessage()
    {
        StringWriter output = new();
        Console.SetOut(output);

        ConsoleEx.WriteLineError("Error message");

        string result = output.ToString();
        Assert.Contains("Error message", result);

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
        Assert.Contains("Event message", result);

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
        Assert.Contains("Tool message", result);

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
        Assert.Contains("Output message", result);

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
        Assert.Equal(ConsoleColor.Green, ConsoleEx.ToolColor);
        Assert.Equal(ConsoleColor.Red, ConsoleEx.ErrorColor);
        Assert.Equal(ConsoleColor.Cyan, ConsoleEx.OutputColor);
        Assert.Equal(ConsoleColor.Yellow, ConsoleEx.EventColor);
    }
}
