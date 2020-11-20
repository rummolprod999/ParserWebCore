using System;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderAbsGroup : TenderAbstract, ITender
    {
        private readonly TypeAbsGroup _tn;

        public TenderAbsGroup(string etpName, string etpUrl, int typeFz, TypeAbsGroup tn) : base(etpName, etpUrl,
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