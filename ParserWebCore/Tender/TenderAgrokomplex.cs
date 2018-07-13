namespace ParserWebCore.Tender
{
    public class TenderAgrokomplex : TenderAbstract, ITender
    {
        public TenderAgrokomplex(string etpName, string etpUrl, int typeFz) : base(etpName, etpUrl, typeFz)
        {
        }

        public void ParsingTender()
        {
        }
    }
}