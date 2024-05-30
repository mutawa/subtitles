using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace subtitles
{
    public class Line
    {
        public Line(string sequence, DateTime startTime, DateTime endTime, string text)
        {
            Sequence = int.Parse(sequence);
            StartTime = startTime;
            EndTime = endTime;
            Duration = (int)(EndTime - StartTime).TotalMilliseconds;
            Text = text.Trim();
        }

        public int Sequence { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Text { get; set; }
        public int Duration { get; set; }
        public string Timing => $"{StartTime:HH:mm:ss,fff} --> {EndTime:HH:mm:ss,fff}";
        public override string ToString()
        {
            return $"{StartTime:HH:mm:ss,fff} --> {EndTime:HH:mm:ss,fff}\r\n{Text}";
        }

        public void Shift(int milliseconds)
        {
            StartTime = StartTime.AddMilliseconds(milliseconds);
            EndTime = EndTime.AddMilliseconds(milliseconds);
        }

        public void Sync(DateTime start1, DateTime stop1, DateTime start2, DateTime stop2)
        {
            StartTime = Map(StartTime, start1, stop1, start2, stop2);
            EndTime = StartTime.AddMilliseconds(Duration);

        }


        private static readonly DateTime origin = "00:00:00,000".ToDateTime();
        public static DateTime Map(DateTime n, DateTime start1, DateTime stop1, DateTime start2, DateTime stop2)
        {
            double d = (n - start1).TotalMilliseconds;
            double total1 = (stop1 - start1).TotalMilliseconds;
            double total2 = (stop2 - start2).TotalMilliseconds;
            double s2 = (start2 - origin).TotalMilliseconds;
            double r = d / total1 * total2 + s2;

            return origin.AddMilliseconds(r);
        }
    }
}
