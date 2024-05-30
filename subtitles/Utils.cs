using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace subtitles
{

    public static class Utils
    {
        public static void Shift(this IEnumerable<Line> lines, int milliseconds)
        {
            foreach (Line line in lines)
            {
                line.Shift(milliseconds);
            }
        }
        public static void Shift(this IEnumerable<Line> lines, int lineNumber, DateTime correctTime)
        {
            var line = lines.Where(l => l.Sequence == lineNumber).FirstOrDefault();
            if (line == null)
            {
                throw new Exception($"line number [{lineNumber}] not found in subtitles");
            }
            else
            {
                Console.WriteLine($"Timestamp of line {lineNumber} in file is {line.StartTime:HH:mm:ss,fff} ");
                int difference = (int)(correctTime - line.StartTime).TotalMilliseconds;
                if (difference == 0)
                {
                    Console.WriteLine("No shifting required");
                }
                else
                {
                    Console.WriteLine($"Shifting all lines by {difference} milliseconds");
                    lines.Shift(difference);
                }
            }
        }
        public static void Sync(this IEnumerable<Line> lines, int startLine, int endLine, DateTime correctEndTime)
        {
            DateTime startTimeInFile = lines.Where(l => l.Sequence == startLine).First().StartTime;
            DateTime endTimeInFile = lines.Where(l => l.Sequence == endLine).First().StartTime;
            var before = lines.Last().StartTime;

            foreach (Line line in lines)
            {
                line.Sync(startTimeInFile, endTimeInFile, startTimeInFile, correctEndTime);
            }

            var after = lines.Last().StartTime;

            int difference = (int)(after - before).TotalSeconds;
            if (difference == 0) Console.WriteLine("No change noticed");
            else Console.WriteLine($"subsync adjusted file to movie difference of [{difference}] seconds.");

        }

        public static void WriteToFile(this IEnumerable<Line> lines, string fileName)
        {
            StringBuilder sb = new();
            int cnt = 1;
            foreach (Line line in lines)
            {
                sb.AppendLine($"{cnt}");
                sb.AppendLine($"{line.Timing}");
                sb.AppendLine($"{line.Text}");
                sb.AppendLine();
                cnt += 1;
            }
            File.WriteAllText(fileName, sb.ToString());
        }

        public static void ReadFile(this List<Line> lines, string inputFile)
        {
            string sourceText;
            bool IsSignatureFound = false;
            bool IsSignInserted = false;

            try
            {
                sourceText = File.ReadAllText(inputFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to access input file [{inputFile}]: {ex.Message}");
            }

            var regex = new Regex(@"(?<sequence>\d+)\r\n(?<start_time>\d\d:\d\d:\d\d,\d\d\d) --\> (?<end_time>\d\d:\d\d:\d\d,\d\d\d)\r\n(?<text>[\s\S]*?\r\n\r\n)", RegexOptions.Compiled | RegexOptions.ECMAScript);
            var sign = new Regex("subsync", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var signMatch = sign.Match(sourceText);
            if (signMatch.Success) { IsSignatureFound = true; }

            var matches = regex.Matches(sourceText);
            int cnt = 0;


            foreach (Match match in matches)
            {
                string sequence = match.Groups["sequence"].Value;
                string startTime = match.Groups["start_time"].Value;
                string endTime = match.Groups["end_time"].Value;

                string text = match.Groups["text"].Value;
                lines.Add(new Line(sequence, startTime.ToDateTime(), endTime.ToDateTime(), text));

                cnt += 1;
                if (IsSignatureFound == false)
                {
                    if (cnt == matches.Count / 2 || cnt == matches.Count)
                    {
                        lines.Add(new Line(sequence + 111, startTime.ToDateTime().AddSeconds(2), endTime.ToDateTime().AddSeconds(4),
                            "subsynced by subsync@abunoor.com"));
                        IsSignInserted = true;
                    }
                }
            }
            if (IsSignInserted) Console.WriteLine("subsync signature inserted");
            

        }

        public static string Find(this string[] args, string field, string defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == field)
                {
                    return args[i + 1];
                }
            }
            return defaultValue;
        }
        public static string[] Between(this string[] args, string key1, string key2)
        {
            var result = new List<string>();

            bool startCollecting = false;
            bool stopCollecting = false;
            bool key2Exists = args.FindKey(key2);

            for (int i = 0; i < args.Length; i++)
            {
                if (startCollecting == false && args[i] == key1)
                {
                    startCollecting = true;
                }
                else if (key2Exists && startCollecting == true && args[i] == key2)
                {
                    stopCollecting = true;
                }
                else if (!key2Exists && startCollecting == true && i == args.Length - 1)
                {
                    result.Add(args[i]);
                    //stopCollecting = true;
                    break;
                }
                else
                {
                    if (stopCollecting) break;
                    if (startCollecting) result.Add(args[i]);
                }
            }

            return result.ToArray();


        }


        public static bool FindKey(this string[] args, string field)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == field)
                {
                    return true;
                }
            }
            return false;
        }
        public static string? FindValue(this string[] args, string field, int offset = 1)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == field)
                {
                    if ((i + offset) < args.Length)
                    {
                        return args[i + offset];
                    }
                    else
                    {
                        Console.WriteLine($"offset [{i + offset}] exceeds arguments length of ({args.Length})");
                        return null;

                    }
                }
            }
            return null;
        }

        public static DateTime ToDateTime(this string text)
        {
            string[] times = text.Split(",");
            return DateTime.Parse(times[0]).AddMilliseconds(int.Parse(times[1]));

        }

        public static bool IsDateTime(this string text)
        {
            return DateTime.TryParse(text, out _);
        }

        public static bool IsTimeStamp(this string text)
        {
            string[] parts = text.Split(",");
            if (parts.Length == 2)
            {
                return parts[0].IsDateTime() && parts[1].IsMilliseconds();
            }
            return false;
        }

        public static bool IsMilliseconds(this string text)
        {
            if (text.Length == 3)
            {
                return int.TryParse(text, out _);
            }
            return false;
        }

        public static bool IsPositiveInteger(this string text)
        {
            int val;
            if (int.TryParse(text, out val))
            {
                return val > 0;
            }
            return false;
        }

        public static bool IsInteger(this string text)
        {

            return int.TryParse(text, out _);

        }

        //public static bool IsEmpty(this string text)
        //{
        //    return string.IsNullOrEmpty(text);
        //}

        public static bool IsEmpty(this string? text)
        {
            if (text == null) return false;
            return string.IsNullOrEmpty(text);
        }


    }
}
