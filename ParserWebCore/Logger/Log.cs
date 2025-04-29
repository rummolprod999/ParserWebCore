#region

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ParserWebCore.BuilderApp;

#endregion

namespace ParserWebCore.Logger
{
    public static class Log
    {
        private static readonly string _fileLog;
        private static readonly object _locker = new object();

        static Log()
        {
            _fileLog = AppBuilder.FileLog;
        }

        public static void Logger(params object[] parametrs)
        {
            var s = "";
            s += DateTime.Now.ToString(CultureInfo.InvariantCulture);
            s = parametrs.Aggregate(s, (current, t) => $"{current} {t}");

            lock (_locker)
            {
                using (var sw = new StreamWriter(_fileLog, true, Encoding.Default))
                {
                    sw.WriteLine(s);
                }
            }
        }
    }
}