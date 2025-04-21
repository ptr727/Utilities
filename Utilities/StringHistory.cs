using System.Collections.Generic;
using System.Text;

namespace InsaneGenius.Utilities;

public class StringHistory
{
    public StringHistory()
    {
    }

    public StringHistory(int maxFirstLines, int maxLastLines)
    {
        MaxFirstLines = maxFirstLines;
        MaxLastLines = maxLastLines;
    }

    public void AppendLine(string value)
    {
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

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        foreach (string item in StringList)
        {
            _ = stringBuilder.AppendLine(item);
        }

        return stringBuilder.ToString();
    }

    public int MaxFirstLines { get; set; }
    public int MaxLastLines { get; set; }
    public List<string> StringList { get; } = [];

    private int _firstLines;
    private int _lastLines;
}
