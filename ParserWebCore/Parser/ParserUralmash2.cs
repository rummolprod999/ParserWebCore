#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    internal class ParserUralmash2 : ParserAbstract, IParser
    {
        private readonly string _href = "https://uralmash-kartex.ru/industrialnyij-park-uralmash-zakypki";

        public void Parsing()
        {
            Parse(ParsingUral2);
        }

        private void ParsingUral2()
        {
            var s = DownloadString.DownLUserAgent(_href);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    _href);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'inpark-cardtender']/div") ??
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
            var purName =
                n.SelectSingleNode("./p[position() = 1]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find purName in {_href}");
            purName = HttpUtility.HtmlDecode(purName);
            var purNum = purName.ToMd5();
            var datePub = DateTime.Now;
            var dateEnd = DateTime.MinValue;
            var attachments = new List<TypeUral2.Attachment>();
            var atts = n.SelectNodes(@".//p/a[@rel = 'noopener']");
            foreach (var a in atts)
            {
                var name = a.InnerText.Trim();
                var url = a.GetAttributeValue("href", "").Trim();
                if (name == "" || url == "")
                {
                    continue;
                }

                url = $"https://uralmash-kartex.ru/{url}";
                attachments.Add(new TypeUral2.Attachment { Name = name, Url = url });
            }

            var tn = new TenderUralmash2("ИНПАРК \"УРАЛМАШ\"",
                "https://uralmash-kartex.ru/industrialnyij-park-uralmash-zakypki", 272,
                new TypeUral2
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = _href, DateEnd = dateEnd,
                    Attachments = attachments
                });
            ParserTender(tn);
        }

        protected override void Parse(Action op)
        {
            op?.Invoke();
        }
    }
}