namespace ParserWebCore.TenderType
{
    public class TypeB2B : AbstractTypeT
    {
        public string OrgName { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(OrgName)}: {OrgName}";
        }
    }
}