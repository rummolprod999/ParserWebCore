using System;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderStniva : TenderAbstract, ITender
    {
        private readonly TypeStniva _tn;

        public TenderStniva(string etpName, string etpUrl, int typeFz, TypeStniva tn) : base(etpName, etpUrl,
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