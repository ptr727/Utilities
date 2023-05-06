using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InsaneGenius.Utilities;

// ISO-639-2 reference
// https://www.loc.gov/standards/iso639-2/langhome.html
// https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt

// T4 template
// https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates
// https://github.com/mono/t4
// wget -O ./Data/ISO-639-2_utf-8.txt https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt
// dotnet tool install -g dotnet-t4
// dotnet publish ./Utilities/Utilities.csproj --self-contained=false --output=./bin/T4
// t4 -P="./bin/T4" --out=./Utilities/Iso6392Gen.cs ./Utilities/Iso6392Gen.tt

// ISO 639-2 class
public partial class Iso6392
{
    public class Record
    {
        // Default to use 2B
        public string Id { get { return Part2B; } }
        // 639-2 Bibliographic
        public string Part2B { get; set; } = "";
        // 639-2 Terminology
        public string Part2T { get; set; } = "";
        // 639-1
        public string Part1 { get; set; } = "";
        // English name
        public string RefName { get; set; } = "";
    }

    public List<Record> RecordList { get; private set; } = new();

    // Create() method is generated from Iso6392Gen.tt
    // public bool Create() { return true; }

    public bool Load(string fileName)
    {
        try
        {
            // Open the file as a stream
            using StreamReader lineReader = new(File.OpenRead(fileName));

            // Init
            RecordList.Clear();

            // Read header
            var line = lineReader.ReadLine();
            Debug.Assert(!string.IsNullOrEmpty(line));

            // Read line by line
            while ((line = lineReader.ReadLine()) is not null)
            {
                // Parse using pipe character
                var records = line.Split('|');
                Debug.Assert(records.Length == 5);

                // Populate record                
                var record = new Record
                {
                    Part2B = records[0].Trim(),
                    Part2T = records[1].Trim(),
                    Part1 = records[2].Trim(),
                    RefName = records[3].Trim(),
                    // Ignore the French name, German name is not in file
                };
                RecordList.Add(record);
            }
        }
        catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }
        return true;
    }

    public Record Find(string languageTag, bool includeDescription)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(languageTag));

        // Find the matching language entry
        Record record = null;

        // 693 3 letter form
        if (languageTag.Length == 3)
        {
            // Try the 639-2/B
            record = RecordList.FirstOrDefault(item => item.Part2B.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
            if (record != null)
                return record;

            // Try the 639-2/T
            record = RecordList.FirstOrDefault(item => item.Part2T.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
            if (record != null)
                return record;
        }

        // 693 2 letter form
        if (languageTag.Length == 2)
        {
            // Try 639-1
            record = RecordList.FirstOrDefault(item => item.Part1.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
            if (record != null)
                return record;
        }

        // Long form
        if (includeDescription)
        {
            // Try long form
            record = RecordList.FirstOrDefault(item => item.RefName.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
            if (record != null)
                return record;
        }

        // Not found
        return null;
    }
}