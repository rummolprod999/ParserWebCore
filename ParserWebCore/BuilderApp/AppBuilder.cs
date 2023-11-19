using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParserWebCore.BuilderApp
{
    public class AppBuilder
    {
        public const string ReqArguments =
            "agrocomplex, kzgroup, agrotomsk, sibintek, setonline, mzvoron, maxi, tver, murman, kalug, smol, samar, udmurt, segezha, akashevo, sitno, naftan, rwby, tekkom, tekmarket, tekmos, mlconf, tekrn, brn32, sportmaster, teksil, sberb2b, zakupmos, agat, rubex, samcom, ravis, boaz, tektkp, zmorts, rtsmarket, uralmash, lotonline, etpu, ismt, tpta, absgroup, rb2b, federal, b2bweb, medsi, gpb, strateg, tenderit, kuzocm, zdship, kopemash, rusfish, uralair, sochipark, korabel, eurosib, spgr, rcs, yangpur, kpresort, stniva, dvina, kursk, ufin, gosyakut, tatar, mts, tverzmo, bash, midural, mordov, kurg, mobwin, acron, udmurtprop, tambov, udsoil, uos, zmk, atisu, rzdmed, progress, toaz, metal100, nazot, famyug, ruscable, etpmir, comrzd, metport, segz, avtodis, stpo, fbrts, miduralgr";

        private static int _port;
        private static AppBuilder _b;

        public static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
            .CodeBase.Substring(5));

        private AppBuilder(string arg)
        {
            GetArgument(arg);
            GetSettings();
            CreateDirs();
        }

        [Required] public static string TempDir { get; set; }
        [Required] public static string LogDir { get; set; }
        [Required] public static string FileLog { get; set; }
        [Required] public static string UserDb { get; set; }
        [Required] public static string PassDb { get; set; }
        [Required] public static string Server { get; set; }
        [Required] public static string Database { get; set; }
        [Required] public static string FederalPass { get; set; }
        [Required] public static string FederalUser { get; set; }
        [Required] public static string SportPass { get; set; }
        [Required] public static string SportUser { get; set; }
        [Required] public static string UfinPass { get; set; }
        [Required] public static string UfinUser { get; set; }
        [Required] public static string SmolPass { get; set; }
        [Required] public static string SmolUser { get; set; }
        [Required] public static string KurgPass { get; set; }
        [Required] public static string KurgUser { get; set; }
        [Required] public static string UdmPass { get; set; }
        [Required] public static string UdmUser { get; set; }

        [Required] public static string SamarPass { get; set; }
        [Required] public static string SamarUser { get; set; }
        [Required] public static string KalugPass { get; set; }
        [Required] public static string KalugUser { get; set; }

        [Required] public static string TambovPass { get; set; }
        [Required] public static string TambovUser { get; set; }
        [Required] public static string MiduralPass { get; set; }
        [Required] public static string MiduralUser { get; set; }

        [Required] public static string MurmPass { get; set; }
        [Required] public static string MurmUser { get; set; }

        [Required] public static string Api { get; set; }
        [Required] public static string Brn32Pass { get; set; }
        [Required] public static string Brn32User { get; set; }
        [Required] public static string DvinaPass { get; set; }
        [Required] public static string DvinaUser { get; set; }
        [Required] public static string MordovPass { get; set; }
        [Required] public static string MordovUser { get; set; }

        [Required] public static string TverZmoPass { get; set; }
        [Required] public static string TverZmoUser { get; set; }
        [Required] public static string ConnectString { get; set; }
        [Required] public static bool UserProxy { get; set; }
        [Required] public static string ProxyFile { get; set; }
        [Required] public static string SpgrPass { get; set; }
        [Required] public static string SpgrUser { get; set; }

        [Required] public static string AgroPass { get; set; }
        [Required] public static string AgroUser { get; set; }
        [Required] public static string FamYugPass { get; set; }
        [Required] public static string FamYugUser { get; set; }

        [Required] public static string FbRtsPass { get; set; }
        [Required] public static string FbRtsUser { get; set; }
        public static string Prefix { get; private set; }
        public static Arguments Arg { get; private set; }

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
                case "zmorts":
                    Arg = Arguments.ZmoRts;
                    break;
                case "rtsmarket":
                    Arg = Arguments.RtsMarket;
                    break;
                case "uralmash":
                    Arg = Arguments.Uralmash;
                    break;
                case "lotonline":
                    Arg = Arguments.LotOnline;
                    break;
                case "etpu":
                    Arg = Arguments.Etpu;
                    break;
                case "dellin":
                    Arg = Arguments.Dellin;
                    break;
                case "ismt":
                    Arg = Arguments.Ismt;
                    break;
                case "tpta":
                    Arg = Arguments.Tpta;
                    break;
                case "absgroup":
                    Arg = Arguments.AbsGroup;
                    break;
                case "rb2b":
                    Arg = Arguments.Rb2b;
                    break;
                case "federal":
                    Arg = Arguments.Federal;
                    break;
                case "b2bweb":
                    Arg = Arguments.B2BWeb;
                    break;
                case "medsi":
                    Arg = Arguments.Medsi;
                    break;
                case "gpb":
                    Arg = Arguments.Gpb;
                    break;
                case "strateg":
                    Arg = Arguments.Strateg;
                    break;
                case "tenderit":
                    Arg = Arguments.Tenderit;
                    break;
                case "kuzocm":
                    Arg = Arguments.Kuzocm;
                    break;
                case "zdship":
                    Arg = Arguments.Zdship;
                    break;
                case "ocontract":
                    Arg = Arguments.Ocontract;
                    break;
                case "pptk":
                    Arg = Arguments.Pptk;
                    break;
                case "dpd":
                    Arg = Arguments.Dpd;
                    break;
                case "kkbank":
                    Arg = Arguments.Kkbank;
                    break;
                case "gipvn":
                    Arg = Arguments.GipVn;
                    break;
                case "bngf":
                    Arg = Arguments.Bngf;
                    break;
                case "workspace":
                    Arg = Arguments.Workspace;
                    break;
                case "kopemash":
                    Arg = Arguments.Kopemash;
                    break;
                case "rusfish":
                    Arg = Arguments.Rusfish;
                    break;
                case "uralair":
                    Arg = Arguments.Uralair;
                    break;
                case "sochipark":
                    Arg = Arguments.Sochipark;
                    break;
                case "korabel":
                    Arg = Arguments.Korabel;
                    break;
                case "eurosib":
                    Arg = Arguments.Eurosib;
                    break;
                case "spgr":
                    Arg = Arguments.Spgr;
                    break;
                case "rcs":
                    Arg = Arguments.Rcs;
                    break;
                case "yangpur":
                    Arg = Arguments.Yangpur;
                    break;
                case "kpresort":
                    Arg = Arguments.Kpresort;
                    break;
                case "stniva":
                    Arg = Arguments.Stniva;
                    break;
                case "lenreg":
                    Arg = Arguments.Lenreg;
                    break;
                case "dvina":
                    Arg = Arguments.Dvina;
                    break;
                case "kursk":
                    Arg = Arguments.Kursk;
                    break;
                case "ufin":
                    Arg = Arguments.Ufin;
                    break;
                case "gosyakut":
                    Arg = Arguments.Gosyakut;
                    break;
                case "tatar":
                    Arg = Arguments.Tatar;
                    break;
                case "mts":
                    Arg = Arguments.Mts;
                    break;
                case "tverzmo":
                    Arg = Arguments.Tverzmo;
                    break;
                case "bash":
                    Arg = Arguments.Bash;
                    break;
                case "midural":
                    Arg = Arguments.Midural;
                    break;
                case "miduralgr":
                    Arg = Arguments.MiduralGr;
                    break;
                case "mordov":
                    Arg = Arguments.Mordov;
                    break;
                case "kurg":
                    Arg = Arguments.Kurg;
                    break;
                case "mobwin":
                    Arg = Arguments.Mobwin;
                    break;
                case "acron":
                    Arg = Arguments.Acron;
                    break;
                case "udmurtprop":
                    Arg = Arguments.UdmurtProp;
                    break;
                case "tambov":
                    Arg = Arguments.Tambov;
                    break;
                case "udsoil":
                    Arg = Arguments.Udsoil;
                    break;
                case "uos":
                    Arg = Arguments.Uos;
                    break;
                case "zmk":
                    Arg = Arguments.Zmk;
                    break;
                case "atisu":
                    Arg = Arguments.Atisu;
                    break;
                case "rzdmed":
                    Arg = Arguments.Rzdmed;
                    break;
                case "progress":
                    Arg = Arguments.Progress;
                    break;
                case "toaz":
                    Arg = Arguments.Toaz;
                    break;
                case "metal100":
                    Arg = Arguments.Metal100;
                    break;
                case "nazot":
                    Arg = Arguments.Nazot;
                    break;
                case "famyug":
                    Arg = Arguments.Famyug;
                    break;
                case "ruscable":
                    Arg = Arguments.Ruscable;
                    break;
                case "etpmir":
                    Arg = Arguments.Etpmir;
                    break;
                case "comrzd":
                    Arg = Arguments.Comrzd;
                    break;
                case "metport":
                    Arg = Arguments.Metport;
                    break;
                case "segz":
                    Arg = Arguments.Segz;
                    break;
                case "avtodis":
                    Arg = Arguments.Avtodis;
                    break;
                case "stpo":
                    Arg = Arguments.Stpo;
                    break;
                case "fbrts":
                    Arg = Arguments.Fbrts;
                    break;
                default:
                    throw new ArgumentException($"Неправильно указан аргумент {s}, используйте {ReqArguments}");
            }
        }

        private static void GetSettings()
        {
            var nameFile = $"{Path}{System.IO.Path.DirectorySeparatorChar}settings.json";
            using (var reader = File.OpenText(nameFile))
            {
                var o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                Prefix = (string)o["prefix"];
                UserDb = (string)o["userdb"];
                PassDb = (string)o["passdb"];
                Server = (string)o["server"];
                FederalPass = (string)o["passfederal"];
                FederalUser = (string)o["userfederal"];
                SportPass = (string)o["passsport"];
                SportUser = (string)o["usersport"];
                SpgrPass = (string)o["passspgr"];
                SpgrUser = (string)o["userspgr"];
                UfinPass = (string)o["passufin"];
                UfinUser = (string)o["userufin"];
                SmolPass = (string)o["passsmol"];
                SmolUser = (string)o["usersmol"];
                KurgPass = (string)o["passkurg"];
                KurgUser = (string)o["userkurg"];
                UdmPass = (string)o["passudm"];
                UdmUser = (string)o["userudm"];
                SamarPass = (string)o["passsamar"];
                SamarUser = (string)o["usersamar"];
                KalugPass = (string)o["passkalug"];
                KalugUser = (string)o["userkalug"];
                TambovPass = (string)o["passtambov"];
                TambovUser = (string)o["usertambov"];
                MiduralPass = (string)o["passmidural"];
                MiduralUser = (string)o["usermidural"];
                MurmPass = (string)o["passmurm"];
                MurmUser = (string)o["usermurm"];
                Api = (string)o["api"];
                Brn32Pass = (string)o["passbrn32"];
                Brn32User = (string)o["userbrn32"];
                DvinaPass = (string)o["passdvina"];
                DvinaUser = (string)o["userdvina"];
                MordovPass = (string)o["passmordov"];
                MordovUser = (string)o["usermordov"];
                TverZmoPass = (string)o["passtverzmo"];
                TverZmoUser = (string)o["usertverzmo"];
                AgroPass = (string)o["passagro"];
                AgroUser = (string)o["useragro"];
                FamYugPass = (string)o["passfamyug"];
                FamYugUser = (string)o["userfamyug"];
                FbRtsPass = (string)o["passfbrts"];
                FbRtsUser = (string)o["userfbrts"];
                UserProxy = (bool)o["use_proxy"];
                ProxyFile = (string)o["proxy_file"];
                _port = int.TryParse((string)o["port"], out _port) ? int.Parse((string)o["port"]) : 3306;
                Database = (string)o["database"];
                var logDirTmp = o["dirs"]
                    .Where(c => ((JObject)c).Properties().First().Name == Arg.ToString().ToLower())
                    .Select(c => (string)c.SelectToken("..log")).First();
                var tempDirTmp = o["dirs"]
                    .Where(c => ((JObject)c).Properties().First().Name == Arg.ToString().ToLower())
                    .Select(c => (string)c.SelectToken("..temp")).First();
                if (string.IsNullOrEmpty(logDirTmp) || string.IsNullOrEmpty(tempDirTmp))
                {
                    throw new Exception("cannot find logDir or tempDir in settings.json");
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

        public static AppBuilder GetBuilder(string arg)
        {
            if (_b == null)
            {
                _b = new AppBuilder(arg);
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