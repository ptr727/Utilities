using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InsaneGenius.Utilities;

// ISO-639-3 reference
// http://www-01.sil.org/iso639-3
// https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab

// T4 template
// https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates
// https://github.com/mono/t4
// wget -O ./Data/iso-639-3.tab https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab
// dotnet tool install -g dotnet-t4
// dotnet publish ./Utilities/Utilities.csproj --self-contained=false --output=./bin/T4
// t4 -P="./bin/T4" --out=./Utilities/Iso6393Gen.cs ./Utilities/Iso6393Gen.tt

// ISO 639-3 class
public partial class Iso6393
{
    public class Record
    {
        // The three-letter 639-3 identifier
        public string Id { get; set; } = "";

        // Equivalent 639-2 identifier of the bibliographic applications code set, if there is one
        public string Part2B { get; set; } = "";

        // Equivalent 639-2 identifier of the terminology applications code set, if there is one
        public string Part2T { get; set; } = "";

        // Equivalent 639-1 identifier, if there is one
        public string Part1 { get; set; } = "";

        // I(ndividual), M(acrolanguage), S(pecial)
        public string Scope { get; set; } = "";

        // A(ncient), C(onstructed), E(xtinct), H(istorical), L(iving), S(pecial)
        public string LanguageType { get; set; } = "";

        // Reference language name
        public string RefName { get; set; } = "";

        // Comment relating to one or more of the columns
        public string Comment { get; set; } = "";
    }

    public List<Record> RecordList { get; private set; } = [];

    // Create() method is generated from Iso6393Gen.tt
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
            string line = lineReader.ReadLine();
            Debug.Assert(!string.IsNullOrEmpty(line));

            // Read line by line
            while ((line = lineReader.ReadLine()) is not null)
            {
                // Parse using tab character
                string[] records = line.Split('\t');
                Debug.Assert(records.Length == 8);

                // Populate record
                Record record = new()
                {
                    Id = records[0].Trim(),
                    Part2B = records[1].Trim(),
                    Part2T = records[2].Trim(),
                    Part1 = records[3].Trim(),
                    Scope = records[4].Trim(),
                    LanguageType = records[5].Trim(),
                    RefName = records[6].Trim(),
                    Comment = records[7].Trim(),
                };
                RecordList.Add(record);
            }
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
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
        // E.g. zho, chi, afr, ger
        if (languageTag.Length == 3)
        {
            // Try 639-3
            record = RecordList.FirstOrDefault(item =>
                item.Id.Equals(languageTag, StringComparison.OrdinalIgnoreCase)
            );
            if (record != null)
            {
                return record;
            }

            // Try the 639-2/B
            record = RecordList.FirstOrDefault(item =>
                item.Part2B.Equals(languageTag, StringComparison.OrdinalIgnoreCase)
            );
            if (record != null)
            {
                return record;
            }

            // Try the 639-2/T
            record = RecordList.FirstOrDefault(item =>
                item.Part2T.Equals(languageTag, StringComparison.OrdinalIgnoreCase)
            );
            if (record != null)
            {
                return record;
            }
        }

        // 693 2 letter form
        // E.g. zh, af, de
        if (languageTag.Length == 2)
        {
            // Try 639-1
            record = RecordList.FirstOrDefault(item =>
                item.Part1.Equals(languageTag, StringComparison.OrdinalIgnoreCase)
            );
            if (record != null)
            {
                return record;
            }
        }

        // Long form
        // E.g. Zambian Sign Language, Zul
        if (includeDescription)
        {
            // Try long form
            record = RecordList.FirstOrDefault(item =>
                item.RefName.Equals(languageTag, StringComparison.OrdinalIgnoreCase)
            );
            if (record != null)
            {
                return record;
            }
        }

        // Not found
        return null;
    }
}
