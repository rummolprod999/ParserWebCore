using ParserWebCore.TenderType;

namespace ParserWebCore.Tender
{
    public class TenderAgroTomsk : TenderAbstract, ITender
    {
        private readonly TypeAgroTomsk _tn;

        public TenderAgroTomsk(string etpName, string etpUrl, int typeFz, TypeAgroTomsk tn) : base(etpName, etpUrl,
            typeFz)
        {
            _tn = tn;
        }

        public void ParsingTender()
        {
        }
    }
}