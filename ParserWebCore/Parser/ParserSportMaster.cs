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
    public class ParserSportMaster : ParserAbstract, IParser
    {
        private readonly string _urlpage = "http://zakupki.sportmaster.ru/list/";

        public void Parsing()
        {
            Parse(ParsingSportMaster);
        }

        private void ParsingSportMaster()
        {
            try
            {
                ParsingPage(_urlpage);
            }
            catch (Exception e)
            {
                Log.Logger(e);
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
                    "//table[@class = 'tenders__announcements-list_table']/tbody/tr") ??
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
            var href = (n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            href = $"http://zakupki.sportmaster.ru{href}";
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            CreateTender(href);
        }

        private protected void CreateTender(string url)
        {
            var s = DownloadString.DownL1251(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var purName = htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {url}");
            var purNum =
                htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Номер']/following-sibling::td")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find purNum in {url}");
            var datePubT =
                htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Дата публикации']/following-sibling::td")?.InnerText
                    ?.Trim() ?? throw new Exception(
                    $"cannot find datePubT in {url}");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT =
                htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Дата окончания приёма заявок']/following-sibling::td")
                    ?.InnerText?.Trim() ?? throw new Exception(
                    $"cannot find dateEndT in {url}");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            var status =
                htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Статус']/following-sibling::td")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find Status in {url}");
            var hrefAttach =
                (htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Пакет документов']/following-sibling::td/a")
                    ?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (!string.IsNullOrEmpty(hrefAttach))
            {
                hrefAttach = $"http://zakupki.sportmaster.ru{hrefAttach}";
            }

            var attachText = htmlDoc.DocumentNode.SelectSingleNode("//td[. = 'Пакет документов']/following-sibling::td")
                ?.InnerText?.Trim() ?? "";
            var attach = new Dictionary<string, string>
            {
                [attachText] = hrefAttach
            };
            var tn = new TenderSportMaster("ООО «Спортмастер»", "http://zakupki.sportmaster.ru/", 216,
                new TypeSportmaster
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = url, DateEnd = dateEnd,
                    Status = status, Attach = attach
                });
            ParserTender(tn);
        }
    }
}