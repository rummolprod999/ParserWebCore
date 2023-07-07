namespace ParserWebCore.TenderType
{
    public class TypeStPo : AbstractTypeT
    {
        public string Region { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Region)}: {Region}";
        }
    }
}