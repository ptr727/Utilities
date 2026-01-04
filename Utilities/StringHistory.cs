using System;
using System.Collections.Generic;

namespace InsaneGenius.Utilities;

/// <summary>
/// Manages a history of strings with configurable limits on the number of first and last lines to retain.
/// </summary>
/// <remarks>
/// This class is useful for maintaining a bounded history buffer, keeping the first N and last M lines
/// while discarding intermediate content when limits are exceeded.
/// </remarks>
public class StringHistory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class with no limits.
    /// </summary>
    public StringHistory() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class with specified limits.
    /// </summary>
    /// <param name="maxFirstLines">Maximum number of first lines to retain.</param>
    /// <param name="maxLastLines">Maximum number of last lines to retain.</param>
    public StringHistory(int maxFirstLines, int maxLastLines)
    {
        MaxFirstLines = maxFirstLines;
        MaxLastLines = maxLastLines;
    }

    /// <summary>
    /// Appends a line to the history, respecting the configured limits.
    /// </summary>
    /// <param name="value">The string value to append.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public void AppendLine(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // No restrictions
        if (MaxFirstLines == 0 && MaxLastLines == 0)
        {
            StringList.Add(value);
            return;
        }

        // Restrict first lines
        if (_firstLines < MaxFirstLines)
        {
            StringList.Add(value);
            _firstLines++;
            return;
        }

        // If MaxLastLines is 0, don't add any more lines after MaxFirstLines
        if (MaxLastLines == 0)
        {
            return;
        }

        // Restrict last lines
        if (_lastLines < MaxLastLines)
        {
            StringList.Add(value);
            _lastLines++;
            return;
        }

        // Roll the last lines
        StringList.RemoveAt(MaxFirstLines);
        StringList.Add(value);
    }

    /// <summary>
    /// Returns all stored lines as a single string with line breaks.
    /// </summary>
    /// <returns>A string containing all stored lines.</returns>
    public override string ToString() =>
        string.Join(Environment.NewLine, StringList)
        + (StringList.Count > 0 ? Environment.NewLine : string.Empty);

    /// <summary>
    /// Gets or sets the maximum number of first lines to retain.
    /// Set to 0 for no limit.
    /// </summary>
    public int MaxFirstLines { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of last lines to retain.
    /// Set to 0 for no limit.
    /// </summary>
    public int MaxLastLines { get; set; }

    /// <summary>
    /// Gets the list of stored strings.
    /// </summary>
    public List<string> StringList { get; } = [];

    private int _firstLines;
    private int _lastLines;
}
