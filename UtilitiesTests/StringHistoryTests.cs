using System;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class StringHistoryTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public void Constructor_Default_ShouldInitialize()
    {
        StringHistory history = new();

        Assert.NotNull(history);
        Assert.Equal(0, history.MaxFirstLines);
        Assert.Equal(0, history.MaxLastLines);
        Assert.Empty(history.StringList);
    }

    [Fact]
    public void Constructor_WithLimits_ShouldSetLimits()
    {
        StringHistory history = new(maxFirstLines: 5, maxLastLines: 3);

        Assert.Equal(5, history.MaxFirstLines);
        Assert.Equal(3, history.MaxLastLines);
        Assert.Empty(history.StringList);
    }

    [Fact]
    public void AppendLine_NoLimits_ShouldAddAllLines()
    {
        StringHistory history = new();

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        Assert.Equal(10, history.StringList.Count);
    }

    [Fact]
    public void AppendLine_WithFirstLinesLimit_ShouldRespectLimit()
    {
        StringHistory history = new(maxFirstLines: 5, maxLastLines: 3);

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        // Should have 5 first + 3 last = 8 lines
        Assert.Equal(8, history.StringList.Count);
        Assert.Equal("Line 0", history.StringList[0]);
        Assert.Equal("Line 4", history.StringList[4]);
        Assert.Equal("Line 9", history.StringList[^1]);
    }

    [Fact]
    public void AppendLine_BeyondLimits_ShouldRollLastLines()
    {
        StringHistory history = new(maxFirstLines: 3, maxLastLines: 2);

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        // Should have 3 first + 2 last = 5 lines
        Assert.Equal(5, history.StringList.Count);

        // First 3 lines
        Assert.Equal("Line 0", history.StringList[0]);
        Assert.Equal("Line 1", history.StringList[1]);
        Assert.Equal("Line 2", history.StringList[2]);

        // Last 2 lines
        Assert.Equal("Line 8", history.StringList[3]);
        Assert.Equal("Line 9", history.StringList[4]);
    }

    [Fact]
    public void AppendLine_WithNull_ShouldThrowArgumentNullException()
    {
        StringHistory history = new();

        _ = Assert.Throws<ArgumentNullException>(() => history.AppendLine(null!));
    }

    [Fact]
    public void ToString_EmptyHistory_ShouldReturnEmptyString()
    {
        StringHistory history = new();

        string result = history.ToString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToString_WithLines_ShouldReturnFormattedString()
    {
        StringHistory history = new();
        history.AppendLine("Line 1");
        history.AppendLine("Line 2");
        history.AppendLine("Line 3");

        string result = history.ToString();

        Assert.Contains("Line 1", result);
        Assert.Contains("Line 2", result);
        Assert.Contains("Line 3", result);
        Assert.EndsWith(Environment.NewLine, result);
    }

    [Fact]
    public void Properties_CanBeModified_ShouldUpdateLimits()
    {
        StringHistory history = new() { MaxFirstLines = 10, MaxLastLines = 5 };

        Assert.Equal(10, history.MaxFirstLines);
        Assert.Equal(5, history.MaxLastLines);
    }

    [Fact]
    public void AppendLine_MultipleSequences_ShouldMaintainCorrectState()
    {
        StringHistory history = new(maxFirstLines: 2, maxLastLines: 2);

        // Add first batch
        history.AppendLine("A");
        history.AppendLine("B");
        Assert.Equal(2, history.StringList.Count);

        // Add more to trigger last lines
        history.AppendLine("C");
        history.AppendLine("D");
        Assert.Equal(4, history.StringList.Count);

        // Add more to trigger rolling
        history.AppendLine("E");
        Assert.Equal(4, history.StringList.Count);

        history.AppendLine("F");
        Assert.Equal(4, history.StringList.Count);

        // Should have: A, B, E, F
        Assert.Equal("A", history.StringList[0]);
        Assert.Equal("B", history.StringList[1]);
        Assert.Equal("E", history.StringList[2]);
        Assert.Equal("F", history.StringList[3]);
    }

    [Fact]
    public void AppendLine_OnlyFirstLinesLimit_ShouldWork()
    {
        StringHistory history = new(maxFirstLines: 3, maxLastLines: 0);

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        // Should have 3 first lines only
        Assert.Equal(3, history.StringList.Count);
        Assert.Equal("Line 0", history.StringList[0]);
        Assert.Equal("Line 1", history.StringList[1]);
        Assert.Equal("Line 2", history.StringList[2]);
    }

    [Fact]
    public void AppendLine_OnlyLastLinesLimit_ShouldWork()
    {
        StringHistory history = new(maxFirstLines: 0, maxLastLines: 3);

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        // Should have 3 last lines only
        Assert.Equal(3, history.StringList.Count);
        Assert.Equal("Line 7", history.StringList[0]);
        Assert.Equal("Line 8", history.StringList[1]);
        Assert.Equal("Line 9", history.StringList[2]);
    }

    [Fact]
    public void ToString_WithSingleLine_ShouldFormat()
    {
        StringHistory history = new();
        history.AppendLine("Single line");

        string result = history.ToString();

        Assert.Equal("Single line" + Environment.NewLine, result);
    }

    [Fact]
    public void AppendLine_EmptyString_ShouldBeAllowed()
    {
        StringHistory history = new();

        history.AppendLine(string.Empty);

        _ = Assert.Single(history.StringList);
        Assert.Equal(string.Empty, history.StringList[0]);
    }

    [Fact]
    public void AppendLine_SpecialCharacters_ShouldPreserve()
    {
        StringHistory history = new();
        string specialLine = "Line with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";

        history.AppendLine(specialLine);

        Assert.Equal(specialLine, history.StringList[0]);
    }

    [Fact]
    public void AppendLine_UnicodeCharacters_ShouldPreserve()
    {
        StringHistory history = new();
        string unicodeLine = "Unicode: 你好世界 🌍🌎🌏";

        history.AppendLine(unicodeLine);

        Assert.Equal(unicodeLine, history.StringList[0]);
    }
}
