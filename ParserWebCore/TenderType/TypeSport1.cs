namespace ParserWebCore.TenderType
{
    public class TypeSport1 : AbstractTypeT
    {
        public string PwName { get; set; }
        public string Person { get; set; }
        public string PhoneEmail { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(PwName)}: {PwName}, {nameof(Person)}: {Person}, {nameof(PhoneEmail)}: {PhoneEmail}";
        }
    }
}