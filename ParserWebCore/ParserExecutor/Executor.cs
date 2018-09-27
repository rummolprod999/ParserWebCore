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
                    _parser = new ParserAgrokomplex();
                    break;
                case Arguments.Kzgroup:
                    _parser = new ParserKzGroup();
                    break;
                case Arguments.Agrotomsk:
                    _parser = new ParserAgroTomsk();
                    break;
                case Arguments.Sibintek:
                    _parser = new ParserSibintek();
                    break;
                case Arguments.Setonline:
                    _parser = new ParserSetOnline();
                    break;
                case Arguments.Mzvoron:
                    _parser = new ParserMzVoron();
                    break;
                case Arguments.Maxi:
                    _parser = new ParserMaxi();
                    break;
                case Arguments.Tver:
                case Arguments.Murman:
                case Arguments.Kalug:
                case Arguments.Smol:
                case Arguments.Samar:
                case Arguments.Udmurt:
                    _parser = new ParserDecoratorGzwSp(arg);
                    break;
                case Arguments.Segezha:
                    _parser = new ParserSegezha();
                    break;
                case Arguments.Akashevo:
                    _parser = new ParserAkashevo();
                    break;
                case Arguments.Sitno:
                    _parser = new ParserSitno();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
            }
        }

        private readonly IParser _parser;

        public void ExecuteParser()
        {
            try
            {
                _parser.Parsing();
            }
            catch (Exception e)
            {
                Logger.Log.Logger("Exception in parsing()", e);
            }
        }
    }
}