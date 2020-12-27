using System;
using ParserWebCore.Connections;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderB2BWeb : TenderAbstract, ITender
    {
        private readonly TypeB2B _tn;

        public TenderB2BWeb(string etpName, string etpUrl, int typeFz, TypeB2B tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            using (var connect = ConnectToDb.GetDbConnection())
            {
                Console.WriteLine(_tn);
            }
        }
    }
}