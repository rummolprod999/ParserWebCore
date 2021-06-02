namespace ParserWebCore.TenderType
{
    public class TypeSochipark : AbstractTypeT
    {
        public string Nmck { get; set; }
        public string PwName { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Nmck)}: {Nmck}, {nameof(PwName)}: {PwName}, {nameof(Currency)}: {Currency}, {nameof(Status)}: {Status}";
        }
    }
}