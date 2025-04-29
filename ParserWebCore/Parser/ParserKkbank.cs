#region

using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserKkbank : ParserAbstract, IParser
    {
        private const int maxPage = 10;
        private readonly string _startPage = "https://kk.bank/o-banke/zakupki/?PAGEN_1=";

        public void Parsing()
        {
            Parse(ParsingKkbank);
        }

        private void ParsingKkbank()
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
            for (var i = 1; i <= maxPage; i++)
            {
                ParsingPage($"{_startPage}{i}");
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'news-item']") ??
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
            var href = (n.SelectSingleNode("./h3/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            ;
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://kk.bank{href}";
            var purName = (n.SelectSingleNode("./h3/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = purName.GetDataFromRegex(@"№\s([\d/-]+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var datePubT =
                (n.SelectSingleNode(".//span[contains(., 'Дата начала -')]")
                    ?.InnerText ?? "").Replace("Дата начала -", "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href, datePubT);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode(".//span[contains(., 'Дата и время окончания приема документов -')]")
                    ?.InnerText ?? "").Replace("Дата и время окончания приема документов -", "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var dateScoringT =
                (n.SelectSingleNode(".//span[contains(., 'Дата подведения итогов -')]")
                    ?.InnerText ?? "").Replace("Дата подведения итогов -", "").Trim();
            var dateScoring = dateScoringT.ParseDateUn("dd.MM.yyyy");
            var tn = new TenderKkBank("Банк «Кубань Кредит»",
                "https://kk.bank/", 312,
                new TypeKkbank
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    ScoringDate = dateScoring
                });
            ParserTender(tn);
        }
    }
}