namespace ParserWebCore.TenderType
{
    public class TypeEtpu : TypeLotOnline
    {
        public string PlacingWay { get; set; }
        public string CusName { get; set; }
        public string CusInn { get; set; }

        public override string ToString() =>
            $"{base.ToString()}, {nameof(PlacingWay)}: {PlacingWay}, {nameof(CusName)}: {CusName}, {nameof(CusInn)}: {CusInn}";
    }
}