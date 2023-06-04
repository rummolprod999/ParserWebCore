namespace ParserWebCore.TenderType
{
    public class TypeFamYug : AbstractTypeT
    {
        public string PlacingWay { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(PlacingWay)}: {PlacingWay}, {nameof(Status)}: {Status}";
        }
    }
}