using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderZakupMos: TenderAbstract, ITender
    {
        private readonly TypeZakupMos _tn;

        public TenderZakupMos(string etpName, string etpUrl, int typeFz, TypeZakupMos tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
        }
    }
}