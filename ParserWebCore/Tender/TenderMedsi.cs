using System;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderMedsi : TenderAbstract, ITender
    {
        private readonly TypeMedsi _tn;

        public TenderMedsi(string etpName, string etpUrl, int typeFz, TypeMedsi tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
            Console.WriteLine(_tn);
        }
    }
}