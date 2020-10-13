using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderIsMt: TenderAbstract, ITender
    {
        private readonly TypeIsMt _tn;

        public TenderIsMt(string etpName, string etpUrl, int typeFz, TypeIsMt tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
        }
    }
}