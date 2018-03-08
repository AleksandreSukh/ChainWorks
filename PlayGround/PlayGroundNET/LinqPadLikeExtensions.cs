using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    namespace TextLoggerNet
    {
        public static class Extensions
        {
            public static string ToValidFileName(this string stringToFileName)
            {
                string result = stringToFileName;
                foreach (var nameChar in Path.GetInvalidFileNameChars())
                    result = result.Replace(nameChar, '_');
                return result;
            }
        }
        public static class TimeSpanVerbose
        {
            public static string ToVerboseStringHMS(this TimeSpan timeSpan)
            {
                var timeParts = new Dictionary<TimePartEnum, uint>();
                timeParts.Add(TimePartEnum.Days, (uint)timeSpan.Days);
                timeParts.Add(TimePartEnum.Hours, (uint)timeSpan.Hours);
                timeParts.Add(TimePartEnum.Minutes, (uint)timeSpan.Minutes);
                timeParts.Add(TimePartEnum.Seconds, (uint)timeSpan.Seconds);

                var nonZeroTimeParts = timeParts.Where(t => t.Value > 0).ToArray();

                var formattedString = string.Empty;
                for (int i = 0; i < nonZeroTimeParts.Length; i++)
                {
                    var timeElement = nonZeroTimeParts[i];

                    if (i != 0)
                        formattedString += " ";

                    formattedString += FormatTimePart(timeElement);
                    if (i == nonZeroTimeParts.Length - 2)
                        formattedString += " And";
                }
                //Return 0 Seconds if no time
                if (formattedString == string.Empty)
                    formattedString += FormatTimePart(new KeyValuePair<TimePartEnum, uint>(TimePartEnum.Seconds, 0));

                return formattedString;
            }

            private static string FormatTimePart(KeyValuePair<TimePartEnum, uint> timeElement)
            {
                return $"{timeElement.Value} {timeElement.Key}";
            }

            enum TimePartEnum
            {
                Days,
                Hours,
                Minutes,
                Seconds
            }

        }

    }

    public static class LinqPadLikeExtensions
    {
        const string Prefix = "//";
        const char After = '=';
        const char AfterEnd = '-';
        private static Action<string> _writeLine;

        static LinqPadLikeExtensions()
        {
            _writeLine = s => Console.WriteLine((string) s);
        }

        public static void Init(Action<string> writerAction)
        {
            _writeLine = writerAction;
        }
        public static void Dump<T>(this Task<T> task)
        {
            task.Result.Dump();
        }
        public static void Dump(this string s)
        {
            _writeLine(DumpFormat(s, Console.WindowWidth));
        }
        public static void Dump(this object obj)
        {
            _writeLine(DumpFormat(obj, Console.WindowWidth));
        }

        public static string DumpFormat(string s, int windowWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine(WriteHeader(windowWidth)); sb.AppendLine($"{Environment.NewLine}{s}{Environment.NewLine}");
            sb.AppendLine(WriteFooter(windowWidth));
            return sb.ToString();
        }

        public static string DumpFormat(object obj, int windowWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine(WriteHeader(windowWidth));
            sb.AppendLine();
            sb.AppendLine(DumpContent(obj));
            sb.AppendLine();
            sb.AppendLine(WriteFooter(windowWidth));
            return sb.ToString();
        }

        private static string DumpContent(object obj)
        {
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            sb.AppendLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
            return sb.ToString();
        }
        private static string WriteHeader(int windowWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Prefix + new String(After, windowWidth - Prefix.Length));
            sb.AppendLine(Prefix);
            return sb.ToString();
        }
        private static string WriteFooter(int windowWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Prefix);
            sb.AppendLine(Prefix + new String(AfterEnd, windowWidth - Prefix.Length));
            return sb.ToString();
        }
    }
}