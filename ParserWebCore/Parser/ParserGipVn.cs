#region

using System;
using System.Globalization;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGipVn : ParserAbstract, IParser
    {
        private static readonly string _url = "http://gipvn.ru/zakupki/";

        public void Parsing()
        {
            Parse(ParsingGipVn);
        }

        private void ParsingGipVn()
        {
            try
            {
                ParsingPage();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }

        private void ParsingPage()
        {
            var s = DownloadString.DownLUserAgent(_url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", _url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//p[contains(., 'Заявки на участие')]") ??
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
            var datePubT =
                (n.SelectSingleNode("./preceding-sibling::b[1]")
                    ?.InnerText ?? "").Replace("&nbsp;", " ").Replace("г.", "").Trim();
            var myCultureInfo = new CultureInfo("ru-RU");
            var datePub = DateTime.Now;
            try
            {
                datePub = DateTime.Parse(datePubT, myCultureInfo);
            }
            catch (Exception)
            {
                // ignored
            }

            var time = n.InnerText.GetDataFromRegex(@"(\d{2}:\d{2})");
            var year = n.InnerText.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var endDateT = $"{year} {time}";
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                year = n.InnerText.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{2})");
                endDateT = $"{year} {time}";
                dateEnd = endDateT.ParseDateUn("dd.MM.yy HH:mm");
            }

            if (dateEnd == DateTime.MinValue)
            {
                datePub.AddDays(2);
            }

            var href = (n.SelectSingleNode("./preceding-sibling::p[a[contains(., 'Пакет')]]/a")?.Attributes["href"]
                    ?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://gipvn.ru{href}";
            var nmck = "";
            var purName =
                (n.SelectSingleNode("./preceding-sibling::node()[contains(., 'Гипровостокнефть')][1]")
                    ?.InnerText ?? "").ReplaceHtmlEntyty().Trim();
            var purNum = purName.ToMd5();
            if (string.IsNullOrEmpty(purName))
            {
                return;
            }

            var tn = new TenderGipVn("АО «Гипровостокнефть»",
                "http://gipvn.ru/", 313,
                new TypeGipVn
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Nmck = nmck
                });
            ParserTender(tn);
        }
    }
}