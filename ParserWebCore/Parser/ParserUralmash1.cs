using System;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserUralmash1 : ParserAbstract, IParser
    {
        private readonly string _href = "https://uralmash-kartex.ru/uralmashzavod-zakypki";

        public void Parsing()
        {
            Parse(ParsingUral1);
        }

        private void ParsingUral1()
        {
            var s = DownloadString.DownLUserAgent(_href);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _href);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'cardtender']/div") ??
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
                n.SelectSingleNode("./div[position() = 2]/p[position() = 1]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find purName in {_href}");
            purName = HttpUtility.HtmlDecode(purName);
            var purNum = purName.ToMd5();
            var pwName =
                n.SelectSingleNode(".//p[span[contains(., 'Способ:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find pwName in {_href}");
            var currency =
                n.SelectSingleNode(".//p[span[contains(., 'Валюта:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find currency in {_href}");
            var dates =
                n.SelectSingleNode(".//p[span[contains(., 'Срок приёма заявок:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find dates in {_href}");
            var datePubT = dates.GetDataFromRegex(@"(^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})");
            var datePub = datePubT.ParseDateUn("yyyy-MM-dd HH:mm:ss");
            var dateEndT = dates.GetDataFromRegex(@"-(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})");
            var dateEnd = dateEndT.ParseDateUn("yyyy-MM-dd HH:mm:ss");
            if (datePub == DateTime.MinValue || dateEnd == DateTime.MinValue)
            {
                throw new Exception(
                    $"Cannot find date pub or date end in {_href}");
            }

            var scoringDate =
                n.SelectSingleNode(".//p[span[contains(., 'Дата подведения итогов:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find scoringDate in {_href}");
            var dateScoring = scoringDate.ParseDateUn("yyyy-MM-dd HH:mm:ss");
            var person =
                n.SelectSingleNode(".//p[span[contains(., 'ФИО ответственного:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find person in {_href}");
            var phone =
                n.SelectSingleNode(".//p[span[contains(., 'Телефон ответственного:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find phone in {_href}");
            var email =
                n.SelectSingleNode(".//p[span[contains(., 'E-mail ответственного:')]]/text()")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find email in {_href}");
            var tn = new TenderUralmash1("УРАЛМАШЗАВОД", "https://uralmash-kartex.ru/uralmashzavod-zakypki", 271,
                new TypeUral1
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = _href, DateEnd = dateEnd,
                    PwName = pwName, Currency = currency, Person = person, Email = email, Phone = phone
                });
            ParserTender(tn);
        }

        protected override void Parse(Action op)
        {
            op?.Invoke();
        }
    }
}