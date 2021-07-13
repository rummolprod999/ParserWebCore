using System;
using System.Linq;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserYangPur : ParserAbstract, IParser
    {
        private const string Urlpage = "http://www.yangpur.ru/pages/48";

        public void Parsing()
        {
            Parse(ParsingYangPur);
        }

        private void ParsingYangPur()
        {
            try
            {
                ParsingPage($"{Urlpage}");
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownL1251(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'staticBlock']") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n)
        {
            var tenders = n.InnerHtml.Split(new string[] {"<hr>"}, StringSplitOptions.None);
            tenders.ToList().ForEach(x =>
            {
                try
                {
                    ParserTender(x);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            });
        }

        private void ParserTender(string n)
        {
            if (n.DelAllWhitespace().Length == 0 || !n.Contains("Приглашение к участию."))
            {
                return;
            }

            var (datePubT, dateEndT) =
                n.GetTwoDataFromRegex(@"Начало:\s*(\d{2}\.\d{2}\.\d{4})\s+окончание:(\d{2}\.\d{2}\.\d{4})");
            if (string.IsNullOrEmpty(datePubT))
            {
                datePubT = n.GetDataFromRegex(@"(?:Начало:|с)\s*(\d+\.\d+\.\d+)");
            }

            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUn("dd.MM.yy");
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUn("d.MM.yyyy");
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUn("d.MM.yy");
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUnRus();
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
                Log.Logger("datePub not found");
            }


            if (string.IsNullOrEmpty(dateEndT))
            {
                dateEndT = n.GetDataFromRegex(@"(?:окончание:|по|до).+?\s*(\d+\.\d+\.\d+)");
            }

            if (string.IsNullOrEmpty(dateEndT))
            {
                var (t1, t2) = n.GetTwoDataFromRegex(@"(\d{2}:\d{2}).+(\d{2}.+\d{4})");
                if (!string.IsNullOrEmpty(t1) && !string.IsNullOrEmpty(t2))
                {
                    dateEndT = $"{t2} {t1}";
                }
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUn("dd.MM.yy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUn("d.MM.yyyy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUn("d.MM.yy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUnRus();
            }

            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
                Log.Logger("dateEnd not found");
            }

            var purName = n.GetDataFromRegex(@"((?:ОАО|ООО).+?)<br>").ReplaceHtmlEntyty();
            var attachments = n.GetAllDataFromRegex(@"href=""(.+?)"">(.+?)</a>");
            var href = attachments.Count > 0 ? attachments[0].url : "http://www.yangpur.ru/pages/48";
            var purNum = purName.ToMd5();
            var tn = new TenderYangPur("«Нефтяная компания «Янгпур»", "http://www.yangpur.ru/", 339,
                new TypeYangPur
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Attachments = attachments,
                });
            ParserTender(tn);
        }
    }
}