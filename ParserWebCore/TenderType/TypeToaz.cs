namespace ParserWebCore.TenderType
{
    public class TypeToaz : AbstractTypeT
    {
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}";
        }
    }
}