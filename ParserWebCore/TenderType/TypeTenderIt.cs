namespace ParserWebCore.TenderType
{
    public class TypeTenderIt : AbstractTypeT
    {
        public string OrgName { get; set; }
        public string City { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(OrgName)}: {OrgName}, {nameof(City)}: {City}";
        }
    }
}