using System;
using ParserWebCore.Logger;

namespace ParserWebCore.Parser
{
    public class ParserFederal : ParserAbstract, IParser
    {
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
        }
    }
}