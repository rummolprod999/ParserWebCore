namespace ParserWebCore.TenderType
{
    public class TypePptk : AbstractTypeT
    {
        public string Status { get; set; }
        public string CusName { get; set; }
        public string PwName { get; set; }
        public string Nmck { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(CusName)}: {CusName}, {nameof(PwName)}: {PwName}, {nameof(Nmck)}: {Nmck}";
        }
    }
}