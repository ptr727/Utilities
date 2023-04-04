using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InsaneGenius.Utilities;

// IETF BCP 47 / RFC 5646 references
// https://en.wikipedia.org/wiki/IETF_language_tag
// https://www.w3.org/International/questions/qa-choosing-language-tags
// https://www.w3.org/International/articles/language-tags/
// https://www.rfc-editor.org/info/bcp47
// https://www.rfc-editor.org/info/rfc4647
// https://www.rfc-editor.org/info/rfc5646

// RFC 5646 data source
// https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry

// BCP 47 libraries
// https://github.com/mattcg/language-subtag-registry
// https://github.com/DanSmith/languagetags-sharp
// https://github.com/rspeer/langcodes

// T4 template
// https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates
// https://github.com/mono/t4
// wget -O language-subtag-registry https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry
// dotnet tool install -g dotnet-t4
// dotnet publish Utilities.csproj --self-contained false --output ./bin/T4
// t4 -P="./bin/T4" --out=Rfc5646Gen.cs Rfc5646Gen.tt

// RFC 5646 class
public partial class Rfc5646
{
    public enum RecordType
    {
        None,
        Language,
        ExtLanguage,
        Script,
        Variant,
        Grandfathered,
        Region,
        Redundant
    }

    public class Record
    {
        public RecordType Type { get; set; } = RecordType.None;
        public string SubTag { get; set; } = "";
        public List<string> Description { get; set; } = new();
        public DateOnly Added { get; set; } = DateOnly.MinValue;
        public string SuppressScript { get; set; } = "";
        public string Scope { get; set; } = "";
        public string MacroLanguage { get; set; } = "";
        public DateOnly Deprecated { get; set; } = DateOnly.MinValue;
        public List<string> Comments { get; set; } = new();
        public List<string> Prefix { get; set; } = new();
        public string PreferredValue { get; set; } = "";
        public string Tag { get; set; } = "";

        public string TagAny { get => string.IsNullOrEmpty(SubTag) ? Tag : SubTag; }
    }

    public DateOnly FileDate { get; private set; } = new();
    public List<Record> RecordList { get; private set; } = new();

    private List<KeyValuePair<string, string>> AttributeList = new();

    // Create() method is generated from Rfc5646Gen.tt
    // public bool Create() { return true; }

    // Load from file
    public bool Load(string fileName)
    {
        try 
        { 
            // Open the file as a stream
            using StreamReader lineReader = new StreamReader(File.OpenRead(fileName));

            // Init
            AttributeList.Clear();
            RecordList.Clear();

            // record-jar format
            // https://datatracker.ietf.org/doc/html/draft-phillips-record-jar-02

            // File-Date: 2023-03-22
            string line = lineReader.ReadLine();
            FileDate = DateFromLine(line);

            // %%
            line = lineReader.ReadLine();
            Debug.Assert(line.Equals("%%"));

            // Read all record attributes until EOF
            while (ReadAttributes(lineReader))
            { 
                // Add record
                RecordList.Add(CreateRecord());
            }

            // Add last record
            RecordList.Add(CreateRecord());
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        { 
            return false;
        }
        return true;
    }

    private bool ReadAttributes(StreamReader lineReader)
    {
        // Clear attribute list
        AttributeList.Clear();

        // Read until %% or EOF
        bool eof = false;
        while (true)
        {
            // Read next line
            string line = lineReader.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                // End of file
                eof = true;
                break;
            }
            if (line.Equals("%%"))
            {
                // End of record
                break;
            }

            // First line should not be multiline
            Debug.Assert(!line.StartsWith("  "));

            // Multiline record starts with two spaces
            // Peek at the next line an look for a space
            while (true)
            {
                // There is no PeekLine(), so we only get 1 char look ahead
                // -1 is EOF or error, else cast to Char
                var peek = lineReader.Peek();
                if (peek == -1 || (Char)peek != ' ')
                {
                    // Done
                    break;
                }

                // Append the next line to the current line
                var multiLine = lineReader.ReadLine();
                Debug.Assert(multiLine.StartsWith("  "));
                line = $"{line.Trim()} {multiLine.Trim()}";
            }

            // Create attribute pair
            var divider = line.IndexOf(':');
            var key = line.Substring(0, divider);
            var value = line.Substring(divider + 1).Trim();

            // Add to attribute list
            AttributeList.Add(new KeyValuePair<string, string>(key, value));
        }

        return !eof;
    }

    private Record CreateRecord()
    {
        var record = new Record();
        foreach (var pair in AttributeList) 
        { 
            switch (pair.Key.ToLower())
            { 
                case "type":
                    record.Type = TypeFromString(pair.Value); 
                    break;
                case "subtag":
                    record.SubTag = pair.Value;
                    break;
                case "description":
                    record.Description.Add(pair.Value);
                    break;
                case "added":
                    record.Added = DateFromString(pair.Value);
                    break;
                case "suppress-script":
                    record.SuppressScript = pair.Value;
                    break;
                case "scope":
                    record.Scope = pair.Value;
                    break;
                case "macrolanguage":
                    record.MacroLanguage = pair.Value;
                    break;
                case "deprecated":
                    record.Deprecated = DateFromString(pair.Value);
                    break;
                case "comments":
                    record.Comments.Add(pair.Value);
                    break;
                case "prefix":
                    record.Prefix.Add(pair.Value);
                    break;
                case "preferred-value":
                    record.PreferredValue = pair.Value;
                    break;
                case "tag":
                    record.Tag = pair.Value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return record;
    }

    private static DateOnly DateFromLine(string line)
    {
        var divider = line.IndexOf(':');
        var key = line.Substring(0, divider);
        var value = line.Substring(divider + 1).Trim();

        return DateFromString(value);
    }

    public static DateOnly DateFromString(string value)
    {
        return DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string StringFromDate(DateOnly value)
    {
        return value.ToString("yyyy-MM-dd");
    }

    public static string ToEncodedString(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static string FromEncodedString(string value)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }

    private RecordType TypeFromString(string value)
    {
        switch (value.ToLower())
        {
            case "language":
                return RecordType.Language;
            case "extlang":
                return RecordType.ExtLanguage;
            case "script":
                return RecordType.Script;
            case "variant":
                return RecordType.Variant;
            case "grandfathered":
                return RecordType.Grandfathered;
            case "region":
                return RecordType.Region;
            case "redundant":
                return RecordType.Redundant;
            default:
                throw new NotImplementedException();
        }
    }

    public Record Find(string language)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(language));

        // Find the matching language entry
        Record record = null;

        // Tag
        record = RecordList.FirstOrDefault(item => item.Tag.Equals(language, StringComparison.OrdinalIgnoreCase));
        if (record != null)
            return record;

        // SubTag
        record = RecordList.FirstOrDefault(item => item.SubTag.Equals(language, StringComparison.OrdinalIgnoreCase));
        if (record != null)
            return record;

        // PreferredValue
        record = RecordList.FirstOrDefault(item => item.PreferredValue.Equals(language, StringComparison.OrdinalIgnoreCase));
        if (record != null)
            return record;

        // Description
        record = RecordList.FirstOrDefault(item => item.Description.Any(description => description.Equals(language, StringComparison.OrdinalIgnoreCase)));
        if (record != null)
            return record;

        // Not found
        return null;
    }
}