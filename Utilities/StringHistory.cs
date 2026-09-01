namespace ptr727.Utilities;

/// <summary>
/// Manages a history of strings with configurable limits on the number of first and last lines to retain.
/// </summary>
/// <remarks>
/// This class is useful for maintaining a bounded history buffer, keeping the first N and last M lines
/// while discarding intermediate content when limits are exceeded. Both limits at zero is the one
/// unrestricted mode, where every appended line is retained; zero on a single side retains no lines
/// on that side. A limit assigned after lines have been appended re-partitions what is already
/// stored, so the history never holds more than the limits then in force allow.
/// </remarks>
public class StringHistory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class with no limits.
    /// </summary>
    public StringHistory() => StringList = _stringList.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class with specified limits.
    /// </summary>
    /// <param name="maxFirstLines">Maximum number of first lines to retain, or 0 to retain none.</param>
    /// <param name="maxLastLines">Maximum number of last lines to retain, or 0 to retain none.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxFirstLines"/> or <paramref name="maxLastLines"/> is negative.
    /// </exception>
    public StringHistory(int maxFirstLines, int maxLastLines)
        : this()
    {
        // Validated here rather than in the setters so the exception names the caller's parameter.
        ArgumentOutOfRangeException.ThrowIfNegative(maxFirstLines);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLastLines);

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
            _stringList.Add(value);
            return;
        }

        // Restrict first lines
        if (_firstLines < MaxFirstLines)
        {
            _stringList.Add(value);
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
            _stringList.Add(value);
            _lastLines++;
            return;
        }

        // Roll the last lines
        _stringList.RemoveAt(MaxFirstLines);
        _stringList.Add(value);
    }

    /// <summary>
    /// Returns all stored lines as a single string with line breaks.
    /// </summary>
    /// <returns>A string containing all stored lines.</returns>
    public override string ToString() =>
        string.Join(Environment.NewLine, _stringList)
        + (_stringList.Count > 0 ? Environment.NewLine : string.Empty);

    /// <summary>
    /// Gets or sets the maximum number of first lines to retain.
    /// Set to 0 to retain no first lines; both limits at 0 retains every line.
    /// </summary>
    /// <remarks>
    /// Assigning this re-partitions the lines already stored against the limits then in force,
    /// which discards whatever the new limits exclude. Setting both limits therefore applies them
    /// one at a time, and the first assignment can discard lines the second would have retained.
    /// Prefer the two-argument constructor when both limits are known up front.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is negative.</exception>
    public int MaxFirstLines
    {
        get => _maxFirstLines;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxFirstLines = value;
            Repartition();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of last lines to retain.
    /// Set to 0 to retain no last lines; both limits at 0 retains every line.
    /// </summary>
    /// <remarks>
    /// Assigning this re-partitions the lines already stored against the limits then in force,
    /// which discards whatever the new limits exclude. Setting both limits therefore applies them
    /// one at a time, and the first assignment can discard lines the second would have retained.
    /// Prefer the two-argument constructor when both limits are known up front.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is negative.</exception>
    public int MaxLastLines
    {
        get => _maxLastLines;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxLastLines = value;
            Repartition();
        }
    }

    /// <summary>
    /// Gets the read-only collection of stored strings.
    /// </summary>
    public ReadOnlyCollection<string> StringList { get; }

    /// <summary>
    /// Brings the stored lines within the current limits and resets the counters to match, so an
    /// append after a limit change continues from what is actually stored.
    /// </summary>
    private void Repartition()
    {
        // Both limits at zero is the unrestricted mode, where every line is retained.
        if (MaxFirstLines == 0 && MaxLastLines == 0)
        {
            _firstLines = 0;
            _lastLines = 0;
            return;
        }

        int firstLines = Math.Min(MaxFirstLines, _stringList.Count);
        int lastLines = Math.Min(MaxLastLines, _stringList.Count - firstLines);

        // Drop the lines between the retained head and the retained tail.
        _stringList.RemoveRange(firstLines, _stringList.Count - firstLines - lastLines);

        _firstLines = firstLines;
        _lastLines = lastLines;
    }

    private readonly List<string> _stringList = [];
    private int _maxFirstLines;
    private int _maxLastLines;
    private int _firstLines;
    private int _lastLines;
}
