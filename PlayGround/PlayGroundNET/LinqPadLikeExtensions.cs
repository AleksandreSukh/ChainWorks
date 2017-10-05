using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utils
{
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