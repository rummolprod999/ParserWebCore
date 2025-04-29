#region

using System;
using ParserWebCore.Logger;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserSochi : IParser
    {
        public void Parsing()
        {
            try
            {
                new ParserSochiPark2().Parsing();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }

            try
            {
                new ParserSochipark().Parsing();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }
    }
}