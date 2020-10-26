namespace ParserWebCore.TenderType
{
    public class TypeObjectTpta
    {
        public string Name { get; set; }
        public string Okei { get; set; }
        public string Quantity { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Okei)}: {Okei}, {nameof(Quantity)}: {Quantity}";
        }
    }
}