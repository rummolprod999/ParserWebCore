namespace ParserWebCore.TenderType
{
    public class TypeKpResort : AbstractTypeT
    {
        public string Nmck { get; set; }
        public string Status { get; set; }
        public string PwName { get; set; }
        public string Currency { get; set; }
        public string DelivTerm { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(Status)}: {Status}, {nameof(PwName)}: {PwName}, {nameof(Currency)}: {Currency}, {nameof(DelivTerm)}: {DelivTerm}";
        }
    }
}