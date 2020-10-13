using System;
using ParserWebCore.Logger;

namespace ParserWebCore.Parser
{
    public class ParserIsMt: ParserAbstract, IParser
    {
        private const int Count = 5;
        public void Parsing()
        {
            Parse(ParsingIsmt);
        }

        private void ParsingIsmt()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://is-mt.pro/Purchase/ListPurchase?Page={i}&SearchType=Purchase";
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
        }
    }
}