namespace ParserWebCore.TenderType
{
    public class TypeSochipark : AbstractTypeT
    {
        public string Nmck { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Nmck)}: {Nmck}";
        }
    }
}