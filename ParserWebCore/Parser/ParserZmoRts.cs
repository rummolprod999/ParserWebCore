using System;
using System.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    public class ParserZmoRts: ParserAbstract, IParser
    {
        private readonly int _countPage = 5;
        private readonly string _apiUrl = "https://zmo-new-webapi.rts-tender.ru/market/api/v1/trades/publicsearch2";
        private readonly int[] _sections = { 162, 168, 196, 198, 248, 220, 190, 218, 216, 222, 190, 204};
        public void Parsing()
        {
            Parse(ParsingZmoRts);
        }

        private void ParsingZmoRts()
        {
            _sections.ToList().ForEach(x =>
            {
                for (int i = 1; i < _countPage; i++)
                {
                    try
                    {
                        GetPage(i, in x);
                    }
                    catch (Exception e)
                    {
                        Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                    }
                }
            });
            
        }

        private void GetPage(int num, in int section)
        {
            var data = "{\"FilterSource\":1,\"Paging\":{\"Page\":" + num + ",\"ItemsPerPage\":9},\"PaginationEventType\":0,\"Sorting\":[{\"field\":\"PublicationDate\",\"title\":\"По новизне\",\"direction\":\"Descending\",\"active\":true}],\"Filtering\":[{\"Title\":\"Регионы поставки\",\"ShortName\":\"regs\",\"Type\":0,\"Value\":[],\"Name\":\"KladrCodeRegions\"},{\"Title\":\"Тип поиска\",\"ShortName\":\"t\",\"Type\":1,\"Value\":1,\"Name\":\"MarketSearchAction\"},{\"Title\":\"Окпд2 коды\",\"ShortName\":\"okpd2s\",\"Type\":0,\"Value\":[],\"Name\":\"Okpd2Codes\"},{\"Title\":\"Организации\",\"ShortName\":\"orgs\",\"Type\":0,\"Value\":[],\"Name\":\"Organizations\"},{\"Title\":\"Статус\",\"ShortName\":\"sts\",\"Type\":1,\"Value\":[],\"Name\":\"Statuses\"}]}";
            var s = DownloadString.DownLRtsZmo(_apiUrl, data, section);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _apiUrl);
                return;
            }
            Console.WriteLine(s);
            
        }
    }
    
}