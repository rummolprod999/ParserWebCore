using System;
using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderFederal : TenderAbstract, ITender
    {
        private readonly TypeFederal _tn;

        public TenderFederal(string etpName, string etpUrl, int typeFz, TypeFederal tn) : base(etpName, etpUrl,
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