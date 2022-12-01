using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ultimate_LVD_data
{
    public static class Util
    {
        public static string[] SplitCSV(string input)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);
            List<string> list = new List<string>();
            string m = null;
            foreach (Match match in csvSplit.Matches(input))
            {
                m = match.Value;
                if (0 == m.Length)
                {
                    list.Add("");
                }

                list.Add(m.TrimStart(','));
            }

            return list.ToArray();
        }
    }
}
