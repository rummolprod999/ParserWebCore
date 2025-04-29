#region

using System;
using ParserWebCore.Logger;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserUralmash : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingUral);
        }

        private void ParsingUral()
        {
            try
            {
                new ParserUralmash1().Parsing();
            }
            catch (Exception e)
            {
                Log.Logger("Exception in parsing()", e);
            }

            try
            {
                new ParserUralmash2().Parsing();
            }
            catch (Exception e)
            {
                Log.Logger("Exception in parsing()", e);
            }
        }
    }
}