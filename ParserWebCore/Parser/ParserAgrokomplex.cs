using System;
using ParserWebCore.Tender;

namespace ParserWebCore.Parser
{
    public class ParserAgrokomplex : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingAgrocomplex);
        }

        private void ParsingAgrocomplex()
        {
        }

        public void ParserTender(ITender t)
        {
            try
            {
                t.ParsingTender();
            }
            catch (Exception e)
            {
                Logger.Log.Logger($"Exeption in {t.GetType()}", e);
            }
        }
    }
}