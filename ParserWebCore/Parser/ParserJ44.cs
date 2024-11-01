using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using ParserWebCore.Connections;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserJ44 : ParserAbstract, IParser
    {
        private const int PageCount = 100;
        

        private readonly List<string> _listUrls = new List<string>
        {
            $"https://zakupki.gov.ru/epz/order/extendedsearch/results.html?morphology=on&search-filter=+%D0%94%D0%B0%D1%82%D0%B5+%D0%BE%D0%B1%D0%BD%D0%BE%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D1%8F&sortDirection=false&recordsPerPage=_50&showLotsInfoHidden=false&sortBy=UPDATE_DATE&fz44=on&currencyIdGeneral=-1&publishDateFrom={DateTime.Now.AddMonths(-1):dd.MM.yyyy}&gws=%D0%92%D1%8B%D0%B1%D0%B5%D1%80%D0%B8%D1%82%D0%B5+%D1%82%D0%B8%D0%BF+%D0%B7%D0%B0%D0%BA%D1%83%D0%BF%D0%BA%D0%B8&OrderPlacementSmallBusinessSubject=on&OrderPlacementRnpData=on&OrderPlacementExecutionRequirement=on&orderPlacement94_0=0&orderPlacement94_1=0&orderPlacement94_2=0&pageNumber=",
        };

        public void Parsing()
        {
            Parse(() => _listUrls.ForEach(ParsingPage));
        }

        private void ParsingPage(string u)
        {
            var maxP = MaxPage(Uri.EscapeUriString($"{u}1"));
            for (var i = 1; i <= maxP; i++)
            {
                var url =
                    Uri.EscapeUriString($"{u}{i}");
                try
                {
                    ParserPage(url);
                }
                catch (Exception e)
                {
                    Log.Logger("Error in ParserJ44.ParserPage", e);
                }
            }
        }

        private void ParserPage(string url)
        {
            if (DownloadString.MaxDownload > 1000) return;
            var s = DownloadString.DownLUserAgentEis(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens = htmlDoc.DocumentNode.SelectNodes(
                           "//div[contains(@class, 'search-registry-entry-block')]/div[contains(@class, 'row')][1]") ??
                       new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserLink(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserLink(HtmlNode n)
        {
            if (DownloadString.MaxDownload > 1000) return;
            var url =
                (n.SelectSingleNode(".//div[contains(@class, 'registry-entry__header-mid__number')]/a")
                    ?.Attributes["href"]?.Value ?? "").Trim();
            var purNumT = (n.SelectSingleNode(".//div[contains(@class, 'registry-entry__header-mid__number')]/a")
                ?.Attributes["href"]?.Value ?? "").Trim();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(purNumT)) return;
            var purNum = purNumT.GetDataFromRegex(@"regNumber=(\d+)");
            if (purNum == "")
            {
                Log.Logger("purNum not found");
                return;
            }

            if (DownloadString.MaxDownload > 1000) return;
            url = "https://zakupki.gov.ru" + url.Replace("common-info.html", "event-journal.html");
            var s = DownloadString.DownLUserAgentEis(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserLink()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var sid = s.GetDataFromRegex(@"sid: '(\d+)',");
            if (string.IsNullOrEmpty(sid))
            {
                Log.Logger("Empty sid in ", url);
                return;
            }

            if (DownloadString.MaxDownload > 1000) return;
            var urlEvent =
                $"https://zakupki.gov.ru/epz/order/notice/card/event/journal/list.html?number=&sid={sid}&entityId=&defaultEntityTypes=false&page=1&pageSize=100&qualifier=&sorted=false";
            var s2 = DownloadString.DownLUserAgentEis(urlEvent);
            if (string.IsNullOrEmpty(s2))
            {
                Log.Logger("Empty string in ParserLink()", urlEvent);
                return;
            }

            var htmlDoc2 = new HtmlDocument();
            htmlDoc2.LoadHtml(s2);
            var tens =
                htmlDoc2.DocumentNode.SelectNodes(
                    "//table[@id = 'event']//tbody/tr") ??
                new HtmlNodeCollection(null);

            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTender =
                    $"SELECT count(*) FROM event_log WHERE   notification_number = @notification_number";
                var cmd = new MySqlCommand(selectTender, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@notification_number", purNum);
                var count = (Int64)cmd.ExecuteScalar();
                if (count == tens.Count)
                {
                    return;
                }
            }

            foreach (var t in tens)
            {
                var ev = (htmlDoc2.DocumentNode.SelectSingleNode("//td[2]")?.InnerText ?? "")
                    .Trim();
                var timezone = (htmlDoc2.DocumentNode.SelectSingleNode("//td[1]/span")?.InnerText ?? "")
                    .Trim();
                var dtime = (htmlDoc2.DocumentNode.SelectSingleNode("//td[1]")?.InnerText ?? "")
                    .Trim();
                dtime = dtime.Replace(timezone, "").Trim();
                var dateTime = dtime.ParseDateUn("dd.MM.yyyy HH:mm");
                var tn = new TypeJ()
                {
                    NotificationNumber = purNum, DateTime = dateTime, TimeZone = timezone, TypeFz = "44", Event = ev
                };
                var tender = new TenderJ(tn);
                ParserTender(tender);
            }
        }

        protected int MaxPage(string u)
        {
            if (DownloadString.MaxDownload >= 1000) return 1;
            var s = DownloadString.DownLUserAgentEis(u);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("cannot get first page from EIS", u);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var maxPageS =
                htmlDoc.DocumentNode.SelectSingleNode("//ul[@class = 'pages']/li[last()]/a/span")?.InnerText ?? "1";
            if (int.TryParse(maxPageS, out var maxP))
            {
                return maxP;
            }

            return 10;
        }
    }
}