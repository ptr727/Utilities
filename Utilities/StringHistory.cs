using System;
using System.Collections.Generic;
using System.Text;

namespace InsaneGenius.Utilities
{
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
            if (FirstLines < MaxFirstLines)
            {
                StringList.Add(value);
                FirstLines ++;
                return;
            }

            // Restrict last lines
            if (LastLines < MaxLastLines)
            {
                StringList.Add(value);
                LastLines ++;
                return;
            }

            // Roll the last lines
            StringList.RemoveAt(MaxFirstLines);
            StringList.Add(value);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in StringList)
                stringBuilder.AppendLine(item);
            
            return stringBuilder.ToString();
        }

        public int MaxFirstLines { get; set; }
        public int MaxLastLines { get; set; }
        public List<string> StringList { get; } = new List<string>();

        private int FirstLines;
        private int LastLines;
    }
}
