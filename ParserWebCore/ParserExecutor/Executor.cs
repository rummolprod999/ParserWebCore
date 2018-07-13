using System;
using ParserWebCore.BuilderApp;
using ParserWebCore.Parser;

namespace ParserWebCore.ParserExecutor
{
    public class Executor
    {
        public Executor(Arguments arg)
        {
            switch (arg)
            {
                case Arguments.Agrocomplex:
                    Parser = new ParserAgrokomplex();
                    break;
            }
        }

        public IParser Parser;

        public void ExecuteParser()
        {
            try
            {
                Parser.Parsing();
            }
            catch (Exception e)
            {
                Logger.Log.Logger("Exeption in parsing()", e);
            }
        }
    }
}