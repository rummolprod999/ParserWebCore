using System;
using ParserWebCore.Logger;

namespace ParserWebCore.Parser
{
    public class ParserZakupMos: ParserAbstract, IParser
    {
        private readonly int _countPage = 20;
        public void Parsing()
        {
            Parse(ParsingZakupMos);
        }

        private void ParsingZakupMos()
        {
            for (int i = 1; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(int num)
        {
        }
    }
}