namespace ParserWebCore.TenderType
{
    public class TypeFbRtsPos
    {
        public string Name { get; set; }
        public string Okei { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string Sum { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(Okei)}: {Okei}, {nameof(Quantity)}: {Quantity}, {nameof(Price)}: {Price}, {nameof(Sum)}: {Sum}";
        }
    }
}