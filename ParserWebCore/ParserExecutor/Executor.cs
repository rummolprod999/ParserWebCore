using System;
using ParserWebCore.BuilderApp;
using ParserWebCore.MlConformity;
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
                case Arguments.Brn32:
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
                case Arguments.Naftan:
                    _parser = new ParserNaftan();
                    break;
                case Arguments.Rwby:
                    _parser = new ParserRwBy();
                    break;
                case Arguments.Tekkom:
                    _parser = new ParserTekKom();
                    break;
                case Arguments.Tekmarket:
                    _parser = new ParserTekMarket();
                    break;
                case Arguments.Tekmos:
                    _parser = new ParserTekMos();
                    break;
                case Arguments.Mlconf:
                    _parser = new ParserConformity();
                    break;
                case Arguments.Tekrn:
                    _parser = new ParserTekRn();
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