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
        private static int _port;
        public static string Prefix { get; private set; }
        public static Arguments Arg { get; private set; }
        private static Builder _b;

        public const string ReqArguments =
            "agrocomplex, kzgroup, agrotomsk, sibintek, setonline, mzvoron, maxi, tver, murman, kalug, smol, samar, udmurt, segezha, akashevo, sitno, naftan, rwby, tekkom, tekmarket, tekmos, mlconf, tekrn, brn32, sportmaster, teksil, sberb2b, zakupmos, agat, rubex, samcom, ravis, boaz, tektkp";

        public static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
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
                case "kzgroup":
                    Arg = Arguments.Kzgroup;
                    break;
                case "agrotomsk":
                    Arg = Arguments.Agrotomsk;
                    break;
                case "sibintek":
                    Arg = Arguments.Sibintek;
                    break;
                case "setonline":
                    Arg = Arguments.Setonline;
                    break;
                case "mzvoron":
                    Arg = Arguments.Mzvoron;
                    break;
                case "maxi":
                    Arg = Arguments.Maxi;
                    break;
                case "tver":
                    Arg = Arguments.Tver;
                    break;
                case "murman":
                    Arg = Arguments.Murman;
                    break;
                case "kalug":
                    Arg = Arguments.Kalug;
                    break;
                case "smol":
                    Arg = Arguments.Smol;
                    break;
                case "samar":
                    Arg = Arguments.Samar;
                    break;
                case "udmurt":
                    Arg = Arguments.Udmurt;
                    break;
                case "segezha":
                    Arg = Arguments.Segezha;
                    break;
                case "akashevo":
                    Arg = Arguments.Akashevo;
                    break;
                case "sitno":
                    Arg = Arguments.Sitno;
                    break;
                case "naftan":
                    Arg = Arguments.Naftan;
                    break;
                case "rwby":
                    Arg = Arguments.Rwby;
                    break;
                case "tekkom":
                    Arg = Arguments.Tekkom;
                    break;
                case "tekmarket":
                    Arg = Arguments.Tekmarket;
                    break;
                case "tekmos":
                    Arg = Arguments.Tekmos;
                    break;
                case "mlconf":
                    Arg = Arguments.Mlconf;
                    break;
                case "tekrn":
                    Arg = Arguments.Tekrn;
                    break;
                case "brn32":
                    Arg = Arguments.Brn32;
                    break;
                case "sportmaster":
                    Arg = Arguments.SportMaster;
                    break;
                case "teksil":
                    Arg = Arguments.Teksil;
                    break;
                case "sberb2b":
                    Arg = Arguments.SberB2B;
                    break;
                case "zakupmos":
                    Arg = Arguments.ZakupMos;
                    break;
                case "agat":
                    Arg = Arguments.Agat;
                    break;
                case "rubex":
                    Arg = Arguments.Rubex;
                    break;
                case "samcom":
                    Arg = Arguments.SamCom;
                    break;
                case "ravis":
                    Arg = Arguments.Ravis;
                    break;
                case "boaz":
                    Arg = Arguments.Boaz;
                    break;
                case "tektkp":
                    Arg = Arguments.TekTkp;
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
                _port = int.TryParse((string) o["port"], out _port) ? int.Parse((string) o["port"]) : 3306;
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
                ConnectString =
                    $"Server={Server};port={_port};Database={Database};User Id={UserDb};password={PassDb};CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600;SslMode=none";
                ConnectString =
                    $"Server={Server};port={_port};Database={Database};User Id={UserDb};password={PassDb};CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600;SslMode=none";
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