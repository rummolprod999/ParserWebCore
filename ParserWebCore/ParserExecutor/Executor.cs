using System;
using ParserWebCore.BuilderApp;
using ParserWebCore.MlConformity;
using ParserWebCore.Parser;

namespace ParserWebCore.ParserExecutor
{
    public class Executor
    {
        private readonly IParser _parser;

        public Executor(Arguments arg)
        {
            switch (arg)
            {
                case Arguments.Agrocomplex:
                    _parser = new ParserAgrokomplexNew();
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
                case Arguments.Dvina:
                case Arguments.Kursk:
                case Arguments.Ufin:
                case Arguments.Tverzmo:
                case Arguments.Gosyakut:
                case Arguments.Midural:
                case Arguments.Mordov:
                case Arguments.UdmurtProp:
                case Arguments.Tambov:
                case Arguments.Kurg:
                    _parser = new ParserDecoratorGzwSp(arg);
                    break;
                case Arguments.Segezha:
                    _parser = new ParserSegezhaNew();
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
                    _parser = new ParserTekmarketNew();
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
                case Arguments.SportMaster:
                    _parser = new ParserSportMaster();
                    break;
                case Arguments.Teksil:
                    _parser = new ParserTekSil();
                    break;
                case Arguments.SberB2B:
                    _parser = new ParserSberB2B();
                    break;
                case Arguments.ZakupMos:
                    _parser = new ParserZakupMos();
                    break;
                case Arguments.Agat:
                    _parser = new ParserAgat();
                    break;
                case Arguments.Rubex:
                    _parser = new ParserRubex();
                    break;
                case Arguments.SamCom:
                    _parser = new ParserSamCom();
                    break;
                case Arguments.Ravis:
                    _parser = new ParserRavis();
                    break;
                case Arguments.Boaz:
                    _parser = new ParserBoaz();
                    break;
                case Arguments.TekTkp:
                    _parser = new ParserTekTkp();
                    break;
                case Arguments.ZmoRts:
                    _parser = new ParserZmoRts();
                    break;
                case Arguments.RtsMarket:
                    _parser = new ParserRtsMarket();
                    break;
                case Arguments.Uralmash:
                    _parser = new ParserUralmash();
                    break;
                case Arguments.LotOnline:
                    _parser = new ParserLotOnline();
                    break;
                case Arguments.Etpu:
                    _parser = new ParserEtpu();
                    break;
                case Arguments.Dellin:
                    _parser = new ParserDellin();
                    break;
                case Arguments.Ismt:
                    _parser = new ParserIsMt();
                    break;
                case Arguments.Tpta:
                    _parser = new ParserTpta();
                    break;
                case Arguments.AbsGroup:
                    _parser = new ParserAbsGroup();
                    break;
                case Arguments.Rb2b:
                    _parser = new ParserRb2b();
                    break;
                case Arguments.Federal:
                    _parser = new ParserFederal();
                    break;
                case Arguments.B2BWeb:
                    _parser = new ParserB2BWeb();
                    break;
                case Arguments.Medsi:
                    _parser = new ParserMedsi();
                    break;
                case Arguments.Gpb:
                    _parser = new ParserGpb();
                    break;
                case Arguments.Strateg:
                    _parser = new ParserStrateg();
                    break;
                case Arguments.Tenderit:
                    _parser = new ParserTenderIt();
                    break;
                case Arguments.Kuzocm:
                    _parser = new ParserKuzocm();
                    break;
                case Arguments.Zdship:
                    _parser = new ParserZdShip();
                    break;
                case Arguments.Ocontract:
                    _parser = new ParserOcontract();
                    break;
                case Arguments.Pptk:
                    _parser = new ParserPptk();
                    break;
                case Arguments.Dpd:
                    _parser = new ParserDpd();
                    break;
                case Arguments.Kkbank:
                    _parser = new ParserKkbank();
                    break;
                case Arguments.GipVn:
                    _parser = new ParserGipVn();
                    break;
                case Arguments.Bngf:
                    _parser = new ParserBngf();
                    break;
                case Arguments.Workspace:
                    _parser = new ParserWorkspace();
                    break;
                case Arguments.Kopemash:
                    _parser = new ParserKopeMash();
                    break;
                case Arguments.Rusfish:
                    _parser = new ParserRusfish();
                    break;
                case Arguments.Uralair:
                    _parser = new ParserUralair();
                    break;
                case Arguments.Sochipark:
                    _parser = new ParserSochi();
                    break;
                case Arguments.Korabel:
                    _parser = new ParserKorabel();
                    break;
                case Arguments.Eurosib:
                    _parser = new ParserEurosib();
                    break;
                case Arguments.Spgr:
                    _parser = new ParserSpgr();
                    break;
                case Arguments.Rcs:
                    _parser = new ParserRcs();
                    break;
                case Arguments.Yangpur:
                    _parser = new ParserYangPur();
                    break;
                case Arguments.Kpresort:
                    _parser = new ParserKpResort();
                    break;
                case Arguments.Stniva:
                    _parser = new ParserStniva();
                    break;
                case Arguments.Lenreg:
                    _parser = new ParserLenReg();
                    break;
                case Arguments.Tatar:
                    _parser = new ParserTatar();
                    break;
                case Arguments.Mts:
                    _parser = new ParserMts();
                    break;
                case Arguments.Bash:
                    _parser = new ParserBash();
                    break;
                case Arguments.Mobwin:
                    _parser = new ParserMobWin();
                    break;
                case Arguments.Acron:
                    _parser = new ParserAcron();
                    break;
                case Arguments.Udsoil:
                    _parser = new ParserUdsOil();
                    break;
                case Arguments.Uos:
                    _parser = new ParserUos();
                    break;
                case Arguments.Zmk:
                    _parser = new ParserZmk();
                    break;
                case Arguments.Atisu:
                    _parser = new ParserAtiSu();
                    break;
                case Arguments.Rzdmed:
                    _parser = new ParserRzdMed();
                    break;
                case Arguments.Progress:
                    _parser = new ParserProgress();
                    break;
                case Arguments.Toaz:
                    _parser = new ParserToaz();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
            }
        }

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