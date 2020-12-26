using System;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    public class ParserB2BWeb : ParserAbstract, IParser
    {
        private const int MaxPage = 20;

        public void Parsing()
        {
            Parse(ParsingB2B);
        }

        private void ParsingB2B()
        {
            for (var i = 0; i <= MaxPage; i++)
            {
                try
                {
                    GetPage($"https://www.b2b-center.ru/market/?from={i * 20}");
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(string url)
        {
            var result = DownloadString.DownLUserAgent(url);
            Console.WriteLine(result);
        }
    }
}