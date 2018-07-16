using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ParserWebCore.BuilderApp
{
    public class Builder
    {
        [Required] public static string TempDir { get; set; }
        [Required] public static string LogDir { get; set; }
        [Required] public static string FileLog { get; set; }
        [Required] public static string UserDb { get; set; }
        [Required] public static string PassDb { get; set; }
        [Required] public static string Server { get; set; }
        [Required] public static string Database { get; set; }
        [Required] public static string ConnectString { get; set; }
        public static int Port;
        public static string Prefix { get; set; }
        public static Arguments Arg { get; set; }
        private static Builder _b;
        public static readonly string ReqArguments = "agrocomplex";

        private static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
            .CodeBase.Substring(5));

        private Builder(string arg)
        {
            GetArgument(arg);
            GetSettings();
            CreateDirs();
        }

        private static void GetArgument(string s)
        {
            switch (s)
            {
                case "agrocomplex":
                    Arg = Arguments.Agrocomplex;
                    break;
                default:
                    throw new Exception($"Неправильно указан аргумент {s}, используйте {ReqArguments}");
            }
        }

        private static void GetSettings()
        {
            var nameFile = $"{Path}{System.IO.Path.DirectorySeparatorChar}settings.json";
            using (var reader = File.OpenText(nameFile))
            {
                var o = (JObject) JToken.ReadFrom(new JsonTextReader(reader));
                Prefix = (string) o["prefix"];
                UserDb = (string) o["userdb"];
                PassDb = (string) o["passdb"];
                Server = (string) o["server"];
                Port = Int32.TryParse((string) o["port"], out Port) ? Int32.Parse((string) o["port"]) : 3306;
                Database = (string) o["database"];
                var logDirTmp = o["dirs"]
                    .Where(c => ((JObject) c).Properties().First().Name == Arg.ToString().ToLower())
                    .Select(c => (string) c.SelectToken("..log")).First();
                var tempDirTmp = o["dirs"]
                    .Where(c => ((JObject) c).Properties().First().Name == Arg.ToString().ToLower())
                    .Select(c => (string) c.SelectToken("..temp")).First();
                if (string.IsNullOrEmpty(logDirTmp) || string.IsNullOrEmpty(tempDirTmp))
                {
                    throw new Exception("Can not find logDir or tempDir in settings.json");
                }

                LogDir = $"{Path}{System.IO.Path.DirectorySeparatorChar}{logDirTmp}";
                TempDir = $"{Path}{System.IO.Path.DirectorySeparatorChar}{tempDirTmp}";
                FileLog = $"{LogDir}{System.IO.Path.DirectorySeparatorChar}{Arg}_{DateTime.Now:dd_MM_yyyy}.log";
                ConnectString = $"Server={Server};port={Port};Database={Database};User Id={UserDb};password={PassDb};CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600;SslMode=none";ConnectString = $"Server={Server};port={Port};Database={Database};User Id={UserDb};password={PassDb};CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600;SslMode=none";
            }
        }

        private static void CreateDirs()
        {
            if (Directory.Exists(TempDir))
            {
                var dirInfo = new DirectoryInfo(TempDir);
                dirInfo.Delete(true);
                Directory.CreateDirectory(TempDir);
            }
            else
            {
                Directory.CreateDirectory(TempDir);
            }

            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
        }

        public static Builder GetBuilder(string arg)
        {
            if (_b == null)
            {
                _b = new Builder(arg);
                var results = new List<ValidationResult>();
                var context = new ValidationContext(_b);
                if (!Validator.TryValidateObject(_b, context, results, true))
                {
                    foreach (var error in results)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }

                    Environment.Exit(0);
                }
            }

            return _b;
        }
    }
}