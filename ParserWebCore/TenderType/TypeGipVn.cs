namespace ParserWebCore.TenderType
{
    public class TypeGipVn : AbstractTypeT
    {
        public string Nmck { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Nmck)}: {Nmck}";
        }
    }
}