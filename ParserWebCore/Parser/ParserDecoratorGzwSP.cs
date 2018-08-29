using System;
using System.Collections;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.Parser
{
    public class ParserDecoratorGzwSp : IParser
    {
        private Arguments _arguments;

        public ParserDecoratorGzwSp(Arguments arg)
        {
            _arguments = arg;
        }

        public void Parsing()
        {
            switch (_arguments)
            {
                case Arguments.Tver:
                    new ParserGzwSp("http://www.tver.ru/zakaz/GzwSP/NoticesGrid", "http://www.tver.ru",
                        "МКУ Центр организации торгов города Твери",
                        "http://www.tver.ru/zakaz/GzwSP/NoticesGrid", 85).Parsing();
                    break;
                case Arguments.Murman:
                    new ParserGzwSp("http://gz-murman.ru/site/GzwSP/NoticesGrid", "http://gz-murman.ru",
                        "Комитет государственных закупок Мурманской области",
                        "http://gz-murman.ru/site/GzwSP/NoticesGrid", 86).Parsing();
                    break;
                case Arguments.Kalug:
                    new ParserGzwSp("http://mimz.tender.admoblkaluga.ru/GzwSP/NoticesGrid",
                        "http://mimz.tender.admoblkaluga.ru",
                        "Малые закупки Калужской области",
                        "http://mimz.tender.admoblkaluga.ru/GzwSP/NoticesGrid", 87).Parsing();
                    break;
                case Arguments.Smol:
                    new ParserGzwSp("http://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid",
                        "http://goszakupki.admin-smolensk.ru",
                        "Малые закупки Смоленской области",
                        "http://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid", 88).Parsing();
                    break;
                case Arguments.Samar:
                    new ParserGzwSp("https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid",
                        "https://webtorgi.samregion.ru",
                        "Малые закупки Самарской области",
                        "https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid", 89).Parsing();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(_arguments), _arguments, null);
            }
        }
    }
}