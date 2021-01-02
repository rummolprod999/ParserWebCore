namespace ParserWebCore.TenderType
{
    public class TypeMedsi : AbstractTypeT
    {
        public string OrgContact { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(OrgContact)}: {OrgContact}";
        }
    }
}