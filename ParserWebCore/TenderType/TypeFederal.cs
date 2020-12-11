namespace ParserWebCore.TenderType
{
    public class TypeFederal : AbstractTypeT
    {
        public string Nmck { get; set; }
        public string Status { get; set; }
        public string PwName { get; set; }
        public string CusName { get; set; }
        public string Currency { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(Status)}: {Status}, {nameof(PwName)}: {PwName}, {nameof(CusName)}: {CusName}, {nameof(Currency)}: {Currency}";
        }
    }
}