using System;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    public class ParserFederal : ParserAbstract, IParser
    {
        private readonly CookieService _cookieService = GetCookieServiceFederal.CreateInstance();

        public void Parsing()
        {
            Parse(ParsingFederal);
        }

        private void ParsingFederal()
        {
            try
            {
                GetPage();
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage()
        {
            _cookieService.CookieValue();
        }
    }
}