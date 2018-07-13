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
                    case Arguments.Agrokomplex:
                        Parser = new ParserAgrokomplex();
                        break;
            }
           
        }

        public IParser Parser;

        public void ExecuteParser()
        {
            Parser.Parsing();
        }
    }
}