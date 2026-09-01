namespace ptr727.Utilities;

/// <summary>
/// Manages a history of strings with configurable limits on the number of first and last lines to retain.
/// </summary>
/// <remarks>
/// This class is useful for maintaining a bounded history buffer, keeping the first N and last M lines
/// while discarding intermediate content when limits are exceeded. Zero on a single side retains no
/// lines on that side, and zero on both is the one unrestricted mode, where every appended line is
/// retained. A limit assigned after lines have been appended re-partitions what is already
/// stored, so the history never holds more than the limits then in force allow, except that an
/// assignment leaving both limits at zero enters the unrestricted mode and discards nothing.
/// Re-partitioning only ever discards: once a line has been dropped the head is closed, and a
/// later, larger <see cref="MaxFirstLines"/> raises the ceiling without refilling the head.
/// </remarks>
public class StringHistory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class in the unrestricted
    /// mode, both limits zero, where every appended line is retained.
    /// </summary>
    public StringHistory() => StringList = _stringList.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="StringHistory"/> class with specified limits.
    /// </summary>
    /// <param name="maxFirstLines">Maximum number of first lines to retain, or 0 to retain none on that side.</param>
    /// <param name="maxLastLines">Maximum number of last lines to retain, or 0 to retain none on that side.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxFirstLines"/> or <paramref name="maxLastLines"/> is negative.
    /// </exception>
    public StringHistory(int maxFirstLines, int maxLastLines)
        : this()
    {
        // Validated here so the exception names the caller's own parameter rather than "value".
        ArgumentOutOfRangeException.ThrowIfNegative(maxFirstLines);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLastLines);

        // Assigned to the fields because the two limits apply together.
        // The setters would apply them one at a time and re-partition an empty history twice.
        _maxFirstLines = maxFirstLines;
        _maxLastLines = maxLastLines;
    }

    /// <summary>
    /// Appends a line to the history, respecting the configured limits.
    /// </summary>
    /// <remarks>
    /// The line is not always stored. Where <see cref="MaxLastLines"/> is zero and the head is not
    /// taking it, the line is discarded rather than retained. Where the tail already holds
    /// <see cref="MaxLastLines"/> lines, the oldest retained tail line is evicted to make room,
    /// and that happens whether or not <see cref="MaxFirstLines"/> still has room, since the head
    /// is closed once anything has been discarded and a later, larger limit does not reopen it.
    /// </remarks>
    /// <param name="value">The string value to append.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public void AppendLine(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // No restrictions
        // The line extends the tail once anything has been discarded, and the head otherwise.
        // The counters then keep describing the stored list for a later limit change to read.
        if (MaxFirstLines == 0 && MaxLastLines == 0)
        {
            _stringList.Add(value);
            if (_discarded)
            {
                _lastLines++;
            }
            else
            {
                _firstLines++;
            }

            return;
        }

        // Restrict first lines
        // The head only grows while nothing has been discarded.
        // A line appended after a discard is not a first line, whatever the limit now allows.
        if (!_discarded && _lastLines == 0 && _firstLines < MaxFirstLines)
        {
            _stringList.Add(value);
            _firstLines++;
            return;
        }

        // If MaxLastLines is 0, don't add any more lines after MaxFirstLines
        if (MaxLastLines == 0)
        {
            _discarded = true;
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
        // The head is what is stored rather than the limit, which a widened one would index past.
        _stringList.RemoveAt(_firstLines);
        _stringList.Add(value);
        _discarded = true;
    }

    /// <summary>
    /// Returns all stored lines as a single string, each line followed by
    /// <see cref="Environment.NewLine"/>.
    /// </summary>
    /// <returns>
    /// The stored lines, with a trailing newline after the last, or an empty string where nothing
    /// is stored. The trailing newline means this is not a plain join of the lines.
    /// </returns>
    public override string ToString() =>
        string.Join(Environment.NewLine, _stringList)
        + (_stringList.Count > 0 ? Environment.NewLine : string.Empty);

    /// <summary>
    /// Sets both limits together, re-partitioning the stored lines once against the pair.
    /// </summary>
    /// <param name="maxFirstLines">Maximum number of first lines to retain, or 0 to retain none on that side.</param>
    /// <param name="maxLastLines">Maximum number of last lines to retain, or 0 to retain none on that side.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxFirstLines"/> or <paramref name="maxLastLines"/> is negative.
    /// </exception>
    /// <remarks>
    /// Assigning <see cref="MaxFirstLines"/> and <see cref="MaxLastLines"/> one after the other
    /// re-partitions twice, so the first assignment measures against the other limit's previous
    /// value and can discard lines the final pair would have retained. Which order avoids that,
    /// where either does, depends on the values and on what is stored, so no fixed ordering is
    /// safe. This method assigns both limits before re-partitioning at all, which is what removes
    /// the dependence on order.
    /// Both limits at zero is the one unrestricted mode, so passing zero twice retains every stored
    /// line and every later one rather than discarding them. This applies both limits at once and
    /// never recovers a line already dropped, so widening a limit raises the ceiling without
    /// refilling the head from what is still stored.
    /// </remarks>
    public void SetLimits(int maxFirstLines, int maxLastLines)
    {
        // Validated here so the exception names the caller's own parameter rather than "value".
        ArgumentOutOfRangeException.ThrowIfNegative(maxFirstLines);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLastLines);

        // Assigned to the fields rather than through the setters.
        // Those re-partition once each, which is the order dependence this method removes.
        _maxFirstLines = maxFirstLines;
        _maxLastLines = maxLastLines;
        Repartition();
    }

    /// <summary>
    /// Gets or sets the maximum number of first lines to retain.
    /// Set to 0 to retain no first lines. Every line is retained only when both limits are 0.
    /// </summary>
    /// <remarks>
    /// Assigning this re-partitions the lines already stored against the limits then in force,
    /// which discards whatever the new limits exclude and never recovers a line already dropped.
    /// An assignment leaving both limits at zero is the exception, entering the unrestricted mode
    /// and discarding nothing, so setting this to zero while the other limit is already zero
    /// removes the bound rather than emptying the history.
    /// Setting both limits therefore applies them one at a time, and the first assignment can
    /// discard lines the second would have retained. Use <see cref="SetLimits"/> to apply both at
    /// once, or the two-argument constructor when both limits are known up front.
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
    /// Set to 0 to retain no last lines. Every line is retained only when both limits are 0.
    /// </summary>
    /// <remarks>
    /// Assigning this re-partitions the lines already stored against the limits then in force,
    /// which discards whatever the new limits exclude and never recovers a line already dropped.
    /// An assignment leaving both limits at zero is the exception, entering the unrestricted mode
    /// and discarding nothing, so setting this to zero while the other limit is already zero
    /// removes the bound rather than emptying the history.
    /// Setting both limits therefore applies them one at a time, and the first assignment can
    /// discard lines the second would have retained. Use <see cref="SetLimits"/> to apply both at
    /// once, or the two-argument constructor when both limits are known up front.
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
    /// <remarks>
    /// This is a live view over the history rather than a snapshot, and the same instance is
    /// returned every time, so what a caller holds changes as lines are appended or discarded.
    /// Copy it before enumerating alongside an <see cref="AppendLine"/>, which would otherwise
    /// throw <see cref="InvalidOperationException"/> as any list enumeration does when the list
    /// changes under it. This type carries no thread-safety guarantee.
    /// </remarks>
    public ReadOnlyCollection<string> StringList { get; }

    /// <summary>
    /// Brings the stored lines within the current limits and resets the counters to match, so an
    /// append after a limit change continues from what is actually stored.
    /// </summary>
    private void Repartition()
    {
        // Zero on both limits is the unrestricted mode, which retains every line.
        // Nothing is discarded here, so the counters already describe the stored list.
        if (MaxFirstLines == 0 && MaxLastLines == 0)
        {
            return;
        }

        int storedCount = _stringList.Count;

        // The head is only ever the stream's own first lines.
        // Once a line has been discarded the head is trimmed but never refilled.
        // Before any discard the stored lines are the whole stream, so both sides cut from them.
        int firstLines = Math.Min(MaxFirstLines, _discarded ? _firstLines : storedCount);
        int lastLines;

        if (_discarded)
        {
            lastLines = Math.Min(MaxLastLines, _lastLines);
            _stringList.RemoveRange(firstLines, _firstLines - firstLines);
            _stringList.RemoveRange(firstLines, _lastLines - lastLines);
        }
        else
        {
            lastLines = Math.Min(MaxLastLines, storedCount - firstLines);
            _stringList.RemoveRange(firstLines, storedCount - firstLines - lastLines);
        }

        _firstLines = firstLines;
        _lastLines = lastLines;
        if (_stringList.Count < storedCount)
        {
            _discarded = true;
        }
    }

    private readonly List<string> _stringList = [];
    private int _maxFirstLines;
    private int _maxLastLines;
    private int _firstLines;
    private int _lastLines;
    private bool _discarded;
}
