using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderZmoRts : TenderAbstract, ITender
    {
        private readonly TypeZmoRts _tn;

        public TenderZmoRts(string etpName, string etpUrl, int typeFz, TypeZmoRts tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }
        public void ParsingTender()
        {
        }
    }
}