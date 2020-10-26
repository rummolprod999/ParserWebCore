using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderTpta: TenderAbstract, ITender
    {
        private readonly TypeTpta _tn;

        public TenderTpta(string etpName, string etpUrl, int typeFz, TypeTpta tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
        }
    }
}