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
    public class ParserMetPort : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingMetPort);
        }

        private void ParsingMetPort()
        {
            for (var i = 1; i < 20; i++)
            {
                try
                {
                    ParsingPage($"https://metallportal.com/zakazi?page={i}");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
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
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 'card order']") ??
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
            var purName = (n.SelectSingleNode(".//h5/a")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purName");
                return;
            }

            var purNum = purName.GetDataFromRegex("#(\\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var href =
                n.SelectSingleNode(".//h5/a")?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            var status = (n.SelectSingleNode(".//h5/span")
                ?.InnerText ?? "").Trim();
            var dateEndT =
                (n.SelectSingleNode(".//span[span[. = 'Сроки:']]")
                    ?.InnerText ?? "").Trim().Replace("Сроки:", "").DelAllWhitespace();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            var tn = new TenderMetPort("metallportal.com", "https://metallportal.com/", 400,
                new TypeMetPort
                {
                    DateEnd = dateEnd,
                    DatePub = DateTime.Now,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}