#region

using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserUos : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingUos);
        }

        private void ParsingUos()
        {
            try
            {
                ParsingPage("https://uos.ru/zakupki/");
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
                    "//div[@class = 'item-purchases']") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserList(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserList(HtmlNode n2)
        {
            var href = (n2.SelectSingleNode(".//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://uos.ru{href}";
            var s = DownloadString.DownLUserAgent(href);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserList()", href);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var n = htmlDoc.DocumentNode;
            var purName = (n.SelectSingleNode("//h1")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = purName.ToMd5();
            var dates1 = (n.SelectSingleNode("//div[@class = 'purchases-detail__date']")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var dates = dates1.GetAllDataFromRegex(@"(\d{2}\.\d{2}\.\d{4}).+(\d{2}\.\d{2}\.\d{4})");
            var datePub = dates[0].url.ParseDateUn("dd.MM.yyyy");
            var dateEnd = dates[0].name.ParseDateUn("dd.MM.yyyy");
            var notice = (n.SelectSingleNode("//div[@class = 'user_par_html']")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var attachments = new List<TypeUos.Attachment>();
            var atts = n.SelectNodes(@"//div[@class = 'item-information-file']");
            foreach (var a in atts)
            {
                var name = (a.SelectSingleNode(".//div[@class = 'item-information-file__name']")
                    ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
                var url = (a.SelectSingleNode(".//div[@class = 'item-information-file__right']//a")
                    ?.Attributes["href"].Value.Trim() ?? "").Trim().ReplaceHtmlEntyty();
                if (name == "" || url == "")
                {
                    continue;
                }

                url = $"https://uos.ru{url}";
                attachments.Add(new TypeUos.Attachment { Name = name, Url = url });
            }

            var tn = new TenderUos("АО «Уралоргсинтез»",
                "https://uos.ru/", 382,
                new TypeUos
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Notice = notice,
                    Attachments = attachments
                });
            ParserTender(tn);
        }

        private void ParserTender(HtmlNode n, string href)
        {
            var purName = (n.SelectSingleNode("//h1")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = purName.GetDataFromRegex(@"№\s+([\d/]+)");
            var dates = purName.GetAllDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})-(\d{2}\.\d{2}\.\d{4})");
            var datePub = dates[0].url.ParseDateUn("dd.MM.yyyy");
            var dateEnd = dates[0].name.ParseDateUn("dd.MM.yyyy");
            var notice = (n.SelectSingleNode("//div[@class = 'user_par_html']")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var attachments = new List<TypeUos.Attachment>();
            var atts = n.SelectNodes(@"//table[@class = 'jc_files']//tr[position() > 1]");
            foreach (var a in atts)
            {
                var name = (a.SelectSingleNode("./td[1]")
                    ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
                var url = (a.SelectSingleNode("./td[2]//a")
                    ?.Attributes["href"].Value.Trim() ?? "").Trim().ReplaceHtmlEntyty();
                if (name == "" || url == "")
                {
                    continue;
                }

                url = $"https://uos.ru{url}";
                attachments.Add(new TypeUos.Attachment { Name = name, Url = url });
            }

            var tn = new TenderUos("АО «Уралоргсинтез»",
                "https://uos.ru/", 382,
                new TypeUos
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Notice = notice,
                    Attachments = attachments
                });
            ParserTender(tn);
        }
    }
}