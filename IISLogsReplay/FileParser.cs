
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace IISLogsReplay
{
    public static class FileParser
    {
        public static List<string[]> Parse(string path, char delimiter = ' ', string fileType = ".log", int beginLine = 0)
        {
            List<string[]> result = new List<string[]>();

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, string.Format("*{0}", fileType));
                foreach (var file in files)
                {
                    result.AddRange(ParseFile(file, delimiter, beginLine));
                }
            }
            else //if(File.Exists(path))
            {
                result.AddRange(ParseFile(path, delimiter, beginLine));
            }

            return result;
        }

        private static List<string[]> ParseFile (string filePath, char delimiter = ' ', int beginLine = 0)
        {
            List<string[]> result = new List<string[]>();
            StreamReader sr = null;
            string line;
            int counter = 1;
            
            try
            {
                //opening file
                sr = new StreamReader(filePath);
                line = sr.ReadLine();
                while (line != null)
                {
                    if (counter >= beginLine)
                    {
                        line = ReplaceCharBetweenQuotes(line, delimiter);

                        var delimited = line.Split(delimiter);

                        for (int i = 0; i < delimited.Length; i++)
                        {
                            delimited[i] = ReplaceCharBetweenQuotes(delimited[i], delimiter, false);
                        }

                        result.Add(delimited);
                    }
                    counter++;
                    line = sr.ReadLine();
                }
            }
            finally
            {
                // close streamreader 
                if (sr != null) sr.Close();
            }

            return result;
        }


        private static string ReplaceCharBetweenQuotes(string line, char delimiter = ' ', bool replaceDelimiter = true)
        {
            var reg = new Regex("\".*?\""); //between quotes " we don't check for the delimiter
            const char replaceby = '§';
            var matches = reg.Matches(line);

            if (matches.Count != 0)
            {
                foreach (var item in matches)
                {
                    string s_item = item.ToString();
                    line = (replaceDelimiter) ? 
                        line.Replace(s_item, s_item.Replace(delimiter, replaceby)) :
                        line.Replace(s_item, s_item.Replace(replaceby, delimiter)) ;
                }
            }

            return line;
        }

    }
}
