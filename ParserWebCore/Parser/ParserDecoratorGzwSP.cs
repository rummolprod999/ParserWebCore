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
                    new ParserGzwSPKalug("https://mimz.admoblkaluga.ru/GzwSP/NoticesGrid",
                        "https://mimz.admoblkaluga.ru",
                        "Малые закупки Калужской области",
                        "http://mimz.tender.admoblkaluga.ru/GzwSP/NoticesGrid", 87, _arguments, 40).Parsing();
                    break;
                case Arguments.Smol:
                    new ParserGzwSSmol("https://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://goszakupki.admin-smolensk.ru",
                            "Малые закупки Смоленской области",
                            "http://goszakupki.admin-smolensk.ru/smallpurchases/GzwSP/NoticesGrid", 88, _arguments, 40)
                        .Parsing();
                    break;
                case Arguments.Samar:
                    new ParserGzwSpSamar("https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid",
                        "https://webtorgi.samregion.ru",
                        "Малые закупки Самарской области",
                        "https://webtorgi.samregion.ru/smallpurchases/GzwSP/NoticesGrid", 89, _arguments).Parsing();
                    break;
                case Arguments.Udmurt:
                    new ParserGzwSPUdmurt("https://wt.udmr.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://wt.mfur.ru",
                            "Малые закупки Удмуртской Республики",
                            "https://wt.udmr.ru/smallpurchases/GzwSP/NoticesGrid", 90, _arguments, 10)
                        .Parsing();
                    new ParserGzwSPUdmurtProp("https://wt.udmr.ru/smallpurchases/GzwSP/ProposalsBidsGrid",
                            "https://wt.mfur.ru",
                            "Малые закупки Удмуртской Республики",
                            "https://wt.udmr.ru/smallpurchases/GzwSP/NoticesGrid", 90, _arguments, 10)
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
                    new ParserGzwSPDvina("https://zakupki.dvinaland.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://zakupki.dvinaland.ru",
                            "Малые закупки Архангельский области",
                            "https://zakupki.dvinaland.ru/smallpurchases/GzwSP/NoticesGrid", 349, _arguments, 7)
                        .Parsing();
                    break;
                case Arguments.Kursk:
                    new ParserGzwSp("http://zak.imkursk.ru/smallpurchases/GzwSP/NoticesGrid",
                            "http://zak.imkursk.ru",
                            "Малые закупки Курской области",
                            "http://zak.imkursk.ru/smallpurchases/GzwSP/NoticesGrid", 350, _arguments)
                        .Parsing();
                    break;
                case Arguments.Ufin:
                    new ParserGzwSPUfin("http://goszakaz.ufin48.ru/smallpurchases/GzwSP/NoticesGrid",
                            "http://goszakaz.ufin48.ru",
                            "Малые закупки Липецкой области",
                            "http://goszakaz.ufin48.ru/smallpurchases/GzwSP/NoticesGrid", 351, _arguments, 5)
                        .Parsing();
                    break;
                case Arguments.Gosyakut:
                    new ParserGzwSp("https://www.goszakazyakutia.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://www.goszakazyakutia.ru",
                            "«WEB-Маркет закупок» Республики Саха (Якутия)",
                            "http://market.goszakazyakutia.ru", 76, _arguments, 3)
                        .Parsing();
                    break;
                case Arguments.Tverzmo:
                    new ParserGzwSp("https://www.tver.ru/zakaz/GzwSP/NoticesGrid",
                            "https://www.tver.ru",
                            "ЗМО г. Тверь",
                            "https://www.tver.ru", 353, _arguments, 5)
                        .Parsing();
                    break;
                case Arguments.Midural:
                    new ParserMidural("https://torgi.midural.ru/smallpurchases/GzwSP/ProposalsBidsGrid",
                            "https://torgi.midural.ru",
                            "Малые закупки  Свердловской области",
                            "https://torgi.midural.ru/", 356, _arguments, 15)
                        .Parsing();
                    break;
                case Arguments.Mordov:
                    new ParserGzwSPMordov("https://goszakaz44.e-mordovia.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://goszakaz44.e-mordovia.ru",
                            "Малые закупки  Республики Мордовия",
                            "https://goszakaz44.e-mordovia.ru/", 357, _arguments, 3)
                        .Parsing();
                    break;
                case Arguments.Kurg:
                    new ParserGzwKurgan("https://zakupki.45fin.ru/smallpurchases/GzwSP/NoticesGrid",
                            "https://zakupki.45fin.ru",
                            "Малые закупки  Курганской области",
                            "https://zakupki.45fin.ru", 358, _arguments, 10)
                        .Parsing();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(_arguments), _arguments, null);
            }
        }
    }
}