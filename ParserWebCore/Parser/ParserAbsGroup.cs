using System;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    
        public class ParserAbsGroup : ParserAbstract, IParser
        {
            private string _startPage = "https://tender.absgroup.ru/tenders/?PAGEN_1=";
            private const int maxPage = 5;
            public void Parsing()
            {
                Parse(ParsingMaxi);
            }

            private void ParsingMaxi()
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
            }

        }
}