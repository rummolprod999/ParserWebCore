namespace ParserWebCore.TenderType
{
    public class TypeWorcspace : AbstractTypeT
    {
        public string Status { get; set; }
        public string Nmck { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}, {nameof(Nmck)}: {Nmck}";
        }
    }
}