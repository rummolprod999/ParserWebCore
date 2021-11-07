using System;
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
                    new ParserGzwSp("http://gostorgi.tver.ru/smallpurchases/GzwSP/NoticesGrid",
                        "http://gostorgi.tver.ru",
                        "МКУ Центр организации торгов города Твери",
                        "http://gostorgi.tver.ru/smallpurchases/GzwSP/NoticesGrid", 85, _arguments).Parsing();
                    break;
                case Arguments.Murman:
                    new ParserGzwSp("http://gz-murman.ru/site/GzwSP/NoticesGrid", "http://gz-murman.ru",
                        "Комитет государственных закупок Мурманской области",
                        "http://gz-murman.ru/site/GzwSP/NoticesGrid", 86, _arguments).Parsing();
                    break;
                case Arguments.Kalug:
                    new ParserGzwSp("http://mimz.tender.admoblkaluga.ru/GzwSP/NoticesGrid",
                        "http://mimz.tender.admoblkaluga.ru",
                        "Малые закупки Калужской области",
                        "http://mimz.tender.admoblkaluga.ru/GzwSP/NoticesGrid", 87, _arguments).Parsing();
                    break;
                case Arguments.Smol:
                    new ParserGzwSp("https://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://goszakupki.admin-smolensk.ru",
                            "Малые закупки Смоленской области",
                            "http://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid", 88, _arguments)
                        .Parsing();
                    break;
                case Arguments.Samar:
                    new ParserGzwSpSamar("https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid",
                        "https://webtorgi.samregion.ru",
                        "Малые закупки Самарской области",
                        "https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid", 89, _arguments).Parsing();
                    break;
                case Arguments.Udmurt:
                    new ParserGzwSp("https://wt.udmr.ru/smallpurchases/GzwSP/NoticesGrid",
                            "http://wt.mfur.ru",
                            "Малые закупки Удмуртской Республики",
                            "https://wt.udmr.ru/smallpurchases/GzwSP/NoticesGrid", 90, _arguments)
                        .Parsing();
                    break;
                case Arguments.Brn32:
                    new ParserGzwSp("http://tender32.ru/site/GzwSP/NoticesGrid",
                            "http://tender32.ru",
                            "Электронный магазин Брянской области",
                            "http://tender32.ru/site/GzwSP/NoticesGrid", 193, _arguments)
                        .Parsing();
                    break;
                case Arguments.Dvina:
                    new ParserGzwSp("https://zakupki.dvinaland.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://zakupki.dvinaland.ru",
                            "Малые закупки Архангельский области",
                            "https://zakupki.dvinaland.ru/smallpurchases/GzwSP/NoticesGrid", 349, _arguments)
                        .Parsing();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(_arguments), _arguments, null);
            }
        }
    }
}