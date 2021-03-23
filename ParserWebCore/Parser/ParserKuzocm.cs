using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserKuzocm : ParserAbstract, IParser
    {
        private readonly List<string> Urlpages = new List<string>
        {
            "https://etp.kuzocm.ru/app/LotList/page?LotList.filter=Sdeclared",
            "https://etp.kuzocm.ru/app/LotList/page?LotList.filter=Sapproval&LotList.pageNum.lots.lotsTable=0"
        };

        public void Parsing()
        {
            Parse(ParsingTpta);
        }

        private void ParsingTpta()
        {
            foreach (var urlpage in Urlpages)
            {
                try
                {
                    ParsingPage(urlpage);
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
                htmlDoc.DocumentNode.SelectNodes(
                    "//table[contains(@class, 'contractor_search_table')]//tr[position() > 1]") ??
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
            var href = (n.SelectSingleNode(".//td[2]//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://etp.kuzocm.ru{href}";
            var purName = (n.SelectSingleNode(".//td[2]//a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNumT = (n.SelectSingleNode(".//td[1]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = purNumT.GetDataFromRegex("(.+)\\.lot");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var cusName = (n.SelectSingleNode(".//td[3]")
                ?.InnerText ?? "").ReplaceHtmlEntyty().Trim();
            var tn = new TenderKuzocm("ПАО КУЗОЦМ", "https://etp.kuzocm.ru/", 306,
                new TypeKuzocm
                {
                    CusName = cusName,
                    DateEnd = DateTime.Now,
                    DatePub = DateTime.Now,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                });
            ParserTender(tn);
        }
    }
}