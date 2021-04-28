namespace ParserWebCore.TenderType
{
    public class TypeBnGf : AbstractTypeT
    {
        public string OrgName { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(OrgName)}: {OrgName}, {nameof(Status)}: {Status}";
        }
    }
}