using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ParserWebCore.Logger
{
    public static class Log
    {
        private static string FileLog;
        private static object _locker = new object();

        static Log()
        {
            FileLog = BuilderApp.Builder.FileLog;
        }

        public static void Logger(params object[] parametrs)
        {
            string s = "";
            s += DateTime.Now.ToString(CultureInfo.InvariantCulture);
            s = parametrs.Aggregate(s, (current, t) => $"{current} {t}");

            lock (_locker)
            {
                using (StreamWriter sw = new StreamWriter(FileLog, true, Encoding.Default))
                {
                    sw.WriteLine(s);
                }
            }
        }
    }
}