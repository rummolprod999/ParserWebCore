namespace ParserWebCore.TenderType
{
    public class TypeAbsGroup : AbstractTypeT
    {
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Status)}: {Status}";
        }
    }
}