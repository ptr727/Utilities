namespace ptr727.Utilities.Tests;

public class StringHistoryTests
{
    [Fact]
    public void Constructor_Default_ShouldInitialize()
    {
        StringHistory history = new();

        _ = history.Should().NotBeNull();
        _ = history.MaxFirstLines.Should().Be(0);
        _ = history.MaxLastLines.Should().Be(0);
        _ = history.StringList.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithLimits_ShouldSetLimits()
    {
        StringHistory history = new(maxFirstLines: 5, maxLastLines: 3);

        _ = history.MaxFirstLines.Should().Be(5);
        _ = history.MaxLastLines.Should().Be(3);
        _ = history.StringList.Should().BeEmpty();
    }

    [Fact]
    public void AppendLine_NoLimits_ShouldAddAllLines()
    {
        StringHistory history = new();

        for (int i = 0; i < 10; i++)
        {
            history.AppendLine($"Line {i}");
        }

        _ = history.StringList.Count.Should().Be(10);
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
        _ = history.StringList.Count.Should().Be(8);
        _ = history.StringList[0].Should().Be("Line 0");
        _ = history.StringList[4].Should().Be("Line 4");
        _ = history.StringList[^1].Should().Be("Line 9");
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
        _ = history.StringList.Count.Should().Be(5);

        // First 3 lines
        _ = history.StringList[0].Should().Be("Line 0");
        _ = history.StringList[1].Should().Be("Line 1");
        _ = history.StringList[2].Should().Be("Line 2");

        // Last 2 lines
        _ = history.StringList[3].Should().Be("Line 8");
        _ = history.StringList[4].Should().Be("Line 9");
    }

    [Fact]
    public void AppendLine_WithNull_ShouldThrowArgumentNullException()
    {
        StringHistory history = new();

        _ = FluentActions
            .Invoking(() => history.AppendLine(null!))
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToString_EmptyHistory_ShouldReturnEmptyString()
    {
        StringHistory history = new();

        string result = history.ToString();

        _ = result.Should().Be(string.Empty);
    }

    [Fact]
    public void ToString_WithLines_ShouldReturnFormattedString()
    {
        StringHistory history = new();
        history.AppendLine("Line 1");
        history.AppendLine("Line 2");
        history.AppendLine("Line 3");

        string result = history.ToString();

        _ = result.Should().Contain("Line 1");
        _ = result.Should().Contain("Line 2");
        _ = result.Should().Contain("Line 3");
        _ = result.Should().EndWith(Environment.NewLine);
    }

    [Fact]
    public void Properties_CanBeModified_ShouldUpdateLimits()
    {
        StringHistory history = new() { MaxFirstLines = 10, MaxLastLines = 5 };

        _ = history.MaxFirstLines.Should().Be(10);
        _ = history.MaxLastLines.Should().Be(5);
    }

    [Fact]
    public void AppendLine_MultipleSequences_ShouldMaintainCorrectState()
    {
        StringHistory history = new(maxFirstLines: 2, maxLastLines: 2);

        // Add first batch
        history.AppendLine("A");
        history.AppendLine("B");
        _ = history.StringList.Count.Should().Be(2);

        // Add more to trigger last lines
        history.AppendLine("C");
        history.AppendLine("D");
        _ = history.StringList.Count.Should().Be(4);

        // Add more to trigger rolling
        history.AppendLine("E");
        _ = history.StringList.Count.Should().Be(4);

        history.AppendLine("F");
        _ = history.StringList.Count.Should().Be(4);

        // Should have: A, B, E, F
        _ = history.StringList[0].Should().Be("A");
        _ = history.StringList[1].Should().Be("B");
        _ = history.StringList[2].Should().Be("E");
        _ = history.StringList[3].Should().Be("F");
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
        _ = history.StringList.Count.Should().Be(3);
        _ = history.StringList[0].Should().Be("Line 0");
        _ = history.StringList[1].Should().Be("Line 1");
        _ = history.StringList[2].Should().Be("Line 2");
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
        _ = history.StringList.Count.Should().Be(3);
        _ = history.StringList[0].Should().Be("Line 7");
        _ = history.StringList[1].Should().Be("Line 8");
        _ = history.StringList[2].Should().Be("Line 9");
    }

    [Fact]
    public void ToString_WithSingleLine_ShouldFormat()
    {
        StringHistory history = new();
        history.AppendLine("Single line");

        string result = history.ToString();

        _ = result.Should().Be("Single line" + Environment.NewLine);
    }

    [Fact]
    public void AppendLine_EmptyString_ShouldBeAllowed()
    {
        StringHistory history = new();

        history.AppendLine(string.Empty);

        _ = history.StringList.Should().ContainSingle();
        _ = history.StringList[0].Should().Be(string.Empty);
    }

    [Fact]
    public void AppendLine_SpecialCharacters_ShouldPreserve()
    {
        StringHistory history = new();
        string specialLine = "Line with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";

        history.AppendLine(specialLine);

        _ = history.StringList[0].Should().Be(specialLine);
    }

    [Fact]
    public void AppendLine_UnicodeCharacters_ShouldPreserve()
    {
        StringHistory history = new();
        string unicodeLine = "Unicode: 你好世界 🌍🌎🌏";

        history.AppendLine(unicodeLine);

        _ = history.StringList[0].Should().Be(unicodeLine);
    }
}
